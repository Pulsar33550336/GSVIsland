using ClassIsland.Core.Abstractions.Controls;
using ClassIsland.Shared.Helpers;
using GSVIsland.Models;
using GSVIsland.Shared;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Avalonia.Controls;

namespace GSVIsland.Controls.SpeechProviderSettingsControls;

/// <summary>
/// GSVISpeechServiceSettingsControl.xaml 的交互逻辑
/// </summary>
public partial class GSVISpeechServiceSettingsControl : SpeechProviderControlBase, INotifyPropertyChanged
{
    public GSVISpeechSettings Settings { get; set; } = new();
    
    public GSVISpeechServiceSettingsControl()
    {
        InitializeComponent();
        LoadSettings();
    }

    private void LoadSettings()
    {
        Settings = ConfigureFileHelper.LoadConfig<GSVISpeechSettings>(
            Path.Combine(GlobalConstants.PluginConfigFolder, "Settings.json"));
        
        Settings.PropertyChanged += (sender, args) =>
        {
            ConfigureFileHelper.SaveConfig<GSVISpeechSettings>(
                Path.Combine(GlobalConstants.PluginConfigFolder, "Settings.json"), Settings);
        };
        
        // 设置数据上下文
        this.DataContext = this;
    }

    // 密码变更处理
    private void PasswordChangedHandler(object? sender, TextChangedEventArgs args)
    {
        if (sender is Avalonia.Controls.TextBox textBox)
        {
            Settings.AccessToken = textBox.Text ?? "";
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}