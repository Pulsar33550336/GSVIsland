using CommunityToolkit.Mvvm.ComponentModel;

namespace GSVIsland.Models;

public class GSVISpeechSettings : ObservableRecipient
{
    private string _gSVIServerUrl = "https://gsv2p.acgnai.top";
    private string _accessToken = "";
    private string _version = "v4";
    private string _modelName = "星穹铁道-中文-三月七_ZH";
    private string _promptTextLang = "中文";
    private string _emotion = "默认";
    private string _textLang = "中文";
    private int _topK = 10;
    private double _topP = 1;
    private double _temperature = 1;
    private string _textSplitMethod = "按标点符号切";
    private int _batchSize = 1;
    private double _batchThreshold = 0.75;
    private bool _splitBucket = true;
    private double _speedFactor = 1;
    private double _fragmentInterval = 0.3;
    private string _mediaType = "wav";
    private bool _parallelInfer = true;
    private double _repetitionPenalty = 1.35;
    private int _seed = -1;
    private int _sampleSteps = 16;
    private bool _ifSr = false;

    public string GSVIServerUrl
    {
        get { return _gSVIServerUrl; }
        set
        {
            if (value == _gSVIServerUrl) return;
            _gSVIServerUrl = value;
            OnPropertyChanged();
        }
    }

    public string AccessToken
    {
        get { return _accessToken; }
        set
        {
            if (value == _accessToken) return;
            _accessToken = value;
            OnPropertyChanged();
        }
    }

    public string Version
    {
        get { return _version; }
        set
        {
            if (value == _version) return;
            _version = value;
            OnPropertyChanged();
        }
    }

    public string ModelName
    {
        get { return _modelName; }
        set
        {
            if (value == _modelName) return;
            _modelName = value;
            OnPropertyChanged();
        }
    }

    public string PromptTextLang
    {
        get { return _promptTextLang; }
        set
        {
            if (value == _promptTextLang) return;
            _promptTextLang = value;
            OnPropertyChanged();
        }
    }

    public string Emotion
    {
        get { return _emotion; }
        set
        {
            if (value == _emotion) return;
            _emotion = value;
            OnPropertyChanged();
        }
    }

    public string TextLang
    {
        get { return _textLang; }
        set
        {
            if (value == _textLang) return;
            _textLang = value;
            OnPropertyChanged();
        }
    }

    public int TopK
    {
        get { return _topK; }
        set
        {
            if (value == _topK) return;
            _topK = value;
            OnPropertyChanged();
        }
    }

    public double TopP
    {
        get { return _topP; }
        set
        {
            if (value == _topP) return;
            _topP = value;
            OnPropertyChanged();
        }
    }

    public double Temperature
    {
        get { return _temperature; }
        set
        {
            if (value == _temperature) return;
            _temperature = value;
            OnPropertyChanged();
        }
    }

    public string TextSplitMethod
    {
        get { return _textSplitMethod; }
        set
        {
            if (value == _textSplitMethod) return;
            _textSplitMethod = value;
            OnPropertyChanged();
        }
    }

    public int BatchSize
    {
        get { return _batchSize; }
        set
        {
            if (value == _batchSize) return;
            _batchSize = value;
            OnPropertyChanged();
        }
    }

    public double BatchThreshold
    {
        get { return _batchThreshold; }
        set
        {
            if (value == _batchThreshold) return;
            _batchThreshold = value;
            OnPropertyChanged();
        }
    }

    public bool SplitBucket
    {
        get { return _splitBucket; }
        set
        {
            if (value == _splitBucket) return;
            _splitBucket = value;
            OnPropertyChanged();
        }
    }

    public double SpeedFactor
    {
        get { return _speedFactor; }
        set
        {
            if (value == _speedFactor) return;
            _speedFactor = value;
            OnPropertyChanged();
        }
    }

    public double FragmentInterval
    {
        get { return _fragmentInterval; }
        set
        {
            if (value == _fragmentInterval) return;
            _fragmentInterval = value;
            OnPropertyChanged();
        }
    }

    public string MediaType
    {
        get { return _mediaType; }
        set
        {
            if (value == _mediaType) return;
            _mediaType = value;
            OnPropertyChanged();
        }
    }

    public bool ParallelInfer
    {
        get { return _parallelInfer; }
        set
        {
            if (value == _parallelInfer) return;
            _parallelInfer = value;
            OnPropertyChanged();
        }
    }

    public double RepetitionPenalty
    {
        get { return _repetitionPenalty; }
        set
        {
            if (value == _repetitionPenalty) return;
            _repetitionPenalty = value;
            OnPropertyChanged();
        }
    }

    public int Seed
    {
        get { return _seed; }
        set
        {
            if (value == _seed) return;
            _seed = value;
            OnPropertyChanged();
        }
    }

    public int SampleSteps
    {
        get { return _sampleSteps; }
        set
        {
            if (value == _sampleSteps) return;
            _sampleSteps = value;
            OnPropertyChanged();
        }
    }

    public bool IfSr
    {
        get { return _ifSr; }
        set
        {
            if (value == _ifSr) return;
            _ifSr = value;
            OnPropertyChanged();
        }
    }
}