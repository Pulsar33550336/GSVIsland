using ClassIsland.Core;
using ClassIsland.Core.Abstractions.Services.SpeechService;
using ClassIsland.Core.Attributes;
using ClassIsland.Shared.Helpers;
using GSVIsland.Shared;
using Microsoft.Extensions.Logging;
using SoundFlow;
using SoundFlow.Components;
using SoundFlow.Providers;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClassIsland.Core.Abstractions.Services;
using GSVIsland.Models;
using SoundFlow.Abstracts;
using SoundFlow.Backends.MiniAudio;

namespace GSVIsland.Services.SpeechService;

[SpeechProviderInfo("classisland.speech.GSVI", "GSVI")]
public class GSVIService : ISpeechService
{
    public static readonly string GSVICacheFolderPath = Path.Combine(GlobalConstants.PluginConfigFolder, "GSVICache");

    private ILogger<GSVIService> Logger;

    GSVISpeechSettings Settings { get; set; } = new();

    private Queue<GSVIPlayInfo> PlayingQueue { get; } = new();

    private bool IsPlaying { get; set; } = false;

    private CancellationTokenSource? requestingCancellationTokenSource;

    private SoundPlayer? CurrentSoundPlayer { get; set; }

    private AudioEngine audioEngine;

    private IAudioService AudioService;
    
    public GSVIService(ILogger<GSVIService> logger,IAudioService audioService)
    {
        audioEngine = audioService.AudioEngine;
        AudioService = audioService;
        Logger = logger;
        Settings = ConfigureFileHelper.LoadConfig<GSVISpeechSettings>(Path.Combine(GlobalConstants.PluginConfigFolder,
            "Settings.json"));
        Logger.LogInformation("初始化了 GSVI 服务。");
    }

    public void ReloadConfig()
    {
        Settings = ConfigureFileHelper.LoadConfig<GSVISpeechSettings>(Path.Combine(GlobalConstants.PluginConfigFolder,
            "Settings.json"));
    }

    private HttpClient CreateHttpClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ClassIsland", AppBase.AppVersion));
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        return client;
    }

    private string GetCachePath(string text, string emotion)
    {
        var data = Encoding.UTF8.GetBytes($"{text}_{emotion}");
        var md5 = MD5.HashData(data);
        var md5String = md5.Aggregate("", (current, t) => current + t.ToString("x2"));
        var path = Path.Combine(GSVICacheFolderPath, Settings.ModelName, $"{md5String}.wav");
        var directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory) && directory != null)
        {
            Directory.CreateDirectory(directory);
        }

        return path;
    }

    public void EnqueueSpeechQueue(string text)
    {
        Settings = ConfigureFileHelper.LoadConfig<GSVISpeechSettings>(Path.Combine(GlobalConstants.PluginConfigFolder,
            "Settings.json"));
        var settings = Settings;
        Logger.LogInformation("使用模型 {ModelName}，情感 {Emotion} 朗读文本：{Text}",
            settings.ModelName, settings.Emotion, text);

        if (string.IsNullOrWhiteSpace(text))
        {
            return;
        }

        var previousCts = requestingCancellationTokenSource;
        requestingCancellationTokenSource = new CancellationTokenSource();
        if (previousCts is { IsCancellationRequested: false })
        {
            requestingCancellationTokenSource =
                CancellationTokenSource.CreateLinkedTokenSource(previousCts.Token,
                    requestingCancellationTokenSource.Token);
        }

        var cache = GetCachePath(text, settings.Emotion);
        Logger.LogDebug("语音缓存路径：{CachePath}", cache);

        Task<bool>? task = null;
        if (!File.Exists(cache))
        {
            task = GenerateSpeechAsync(text, cache, requestingCancellationTokenSource.Token);
        }

        if (requestingCancellationTokenSource.IsCancellationRequested)
            return;

        PlayingQueue.Enqueue(new GSVIPlayInfo(cache, new CancellationTokenSource(), task));
        _ = ProcessPlayerList();
    }

    public void ClearSpeechQueue()
    {
        requestingCancellationTokenSource?.Cancel();
        try
        {
            CurrentSoundPlayer?.Stop();
            CurrentSoundPlayer?.Dispose();
            CurrentSoundPlayer = null;
        }
        catch (Exception e)
        {
            // ignored
        }

        while (PlayingQueue.Count > 0)
        {
            var playInfo = PlayingQueue.Dequeue();
            playInfo.CancellationTokenSource.Cancel();
        }
    }

    private async Task<bool> GenerateSpeechAsync(string text, string filePath, CancellationToken cancellationToken)
    {
        var settings = Settings;
        var serverUrl = settings.GSVIServerUrl;

        // 构建请求体
        var requestBody = new
        {
            version = settings.Version,
            model_name = settings.ModelName,
            prompt_text_lang = settings.PromptTextLang,
            emotion = settings.Emotion,
            text = text,
            text_lang = settings.TextLang,
            top_k = settings.TopK,
            top_p = settings.TopP,
            temperature = settings.Temperature,
            text_split_method = settings.TextSplitMethod,
            batch_size = settings.BatchSize,
            batch_threshold = settings.BatchThreshold,
            split_bucket = settings.SplitBucket,
            speed_facter = settings.SpeedFactor,
            fragment_interval = settings.FragmentInterval,
            media_type = settings.MediaType,
            parallel_infer = settings.ParallelInfer,
            repetition_penalty = settings.RepetitionPenalty,
            seed = settings.Seed,
            sample_steps = settings.SampleSteps,
            if_sr = settings.IfSr
        };

        try
        {
            using var httpClient = CreateHttpClient();

            // 设置认证头
            var request = new HttpRequestMessage(HttpMethod.Post, $"{serverUrl}/infer_single");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.AccessToken);

            request.Content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            Logger.LogDebug("发送 GSVI TTS 请求到：{RequestUri}", request.RequestUri);

            using var response =
                await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<GSVIResponse>(responseContent);

                if (result?.Msg == "合成成功" && !string.IsNullOrEmpty(result.AudioUrl))
                {
                    // 下载音频文件
                    using var audioResponse = await httpClient.GetAsync(result.AudioUrl, cancellationToken);
                    if (audioResponse.IsSuccessStatusCode)
                    {
                        await using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write,
                            FileShare.None);
                        await audioResponse.Content.CopyToAsync(fs, cancellationToken);
                        Logger.LogDebug("语音生成并保存到：{FilePath}", filePath);
                        return true;
                    }
                    else
                    {
                        Logger.LogError("下载音频文件失败，状态码：{StatusCode}", audioResponse.StatusCode);
                        return false;
                    }
                }
                else
                {
                    Logger.LogError("TTS 合成失败：{Message}", result?.Msg);
                    return false;
                }
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                Logger.LogError("TTS 请求失败，状态码：{StatusCode}, 内容：{ErrorContent}", response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "发送 TTS 请求时发生异常。");
            return false;
        }
    }

    private async Task ProcessPlayerList()
    {
        if (IsPlaying)
            return;
        IsPlaying = true;

        // 初始化 SoundFlow 音频引擎
        // using var audioEngine = new MiniAudioEngine();

        while (PlayingQueue.Count > 0)
        {
            var playInfo = PlayingQueue.Dequeue();
            if (playInfo.CancellationTokenSource.IsCancellationRequested)
                continue;

            if (playInfo.DownloadTask != null)
            {
                Logger.LogDebug("等待语音生成完成...");
                var result = await playInfo.DownloadTask;
                if (!result)
                {
                    Logger.LogError("语音 {} 生成失败。", playInfo.FilePath);
                    continue;
                }

                Logger.LogDebug("语音生成完成。");
            }

            if (!File.Exists(playInfo.FilePath))
            {
                Logger.LogError("语音文件不存在：{FilePath}", playInfo.FilePath);
                continue;
            }

            CurrentSoundPlayer?.Stop();
            CurrentSoundPlayer?.Dispose();
            using var device = AudioService.TryInitializeDefaultPlaybackDevice();
            device?.Start();
            try
            {
                // 使用 SoundFlow 播放音频
                Stream stream = File.OpenRead(playInfo.FilePath);
                var provider = new StreamDataProvider(audioEngine, IAudioService.DefaultAudioFormat, stream);
                var player = new SoundPlayer(audioEngine, IAudioService.DefaultAudioFormat, provider);
                player.Volume = (float)ISpeechService.GlobalSettings.SpeechVolume;

                CurrentSoundPlayer = player;


                Logger.LogDebug("开始播放 {FilePath}", playInfo.FilePath);
                device?.MasterMixer.AddComponent(player);

                var playbackTcs = new TaskCompletionSource<bool>();

                void PlaybackStoppedHandler(object? sender, EventArgs args)
                {
                    //Mixer.Master.RemoveComponent(player);
                    playInfo.IsPlayingCompleted = true;
                    playbackTcs.SetResult(true);
                }

                player.PlaybackEnded += PlaybackStoppedHandler;
                player.Play();
                playInfo.IsPlayingCompleted = false;

                await playbackTcs.Task;

                player.PlaybackEnded -= PlaybackStoppedHandler;
                Logger.LogDebug("结束播放 {FilePath}", playInfo.FilePath);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "无法播放语音。");
            }
        }

        CurrentSoundPlayer = null;
        IsPlaying = false;
    }
}

// 响应模型
public class GSVIResponse
{
    [JsonPropertyName("msg")]
    public string Msg { get; set; } = "";

    [JsonPropertyName("audio_url")]
    public string AudioUrl { get; set; } = "";
}

// 播放信息类
public class GSVIPlayInfo
{
    public GSVIPlayInfo(string filePath, CancellationTokenSource cancellationTokenSource, Task<bool>? downloadTask = null)
    {
        FilePath = filePath;
        CancellationTokenSource = cancellationTokenSource;
        DownloadTask = downloadTask;
    }

    public string FilePath { get; }
    public CancellationTokenSource CancellationTokenSource { get; }
    public Task<bool>? DownloadTask { get; }
    public bool IsPlayingCompleted { get; set; } = false;
}