using ClassIsland.Shared.Helpers;
using GSVIsland.Shared;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using GSVIsland.Models;

namespace GSVIsland.Controls.SpeechProviderSettingsControls;

/// <summary>
/// GSVISpeechServiceSettingsControl.xaml 的交互逻辑
/// </summary>
public partial class GSVISpeechServiceSettingsControl : INotifyPropertyChanged
{
    public GSVISpeechSettings Settings { get; set; } = new();

    PasswordBox PasswordBox { get; set; } = new();

    public GSVISpeechServiceSettingsControl()
    {
        Settings = ConfigureFileHelper.LoadConfig<GSVISpeechSettings>(Path.Combine(GlobalConstants.PluginConfigFolder, "Settings.json"));
        Settings.PropertyChanged += (sender, args) =>
        {
            ConfigureFileHelper.SaveConfig<GSVISpeechSettings>(Path.Combine(GlobalConstants.PluginConfigFolder, "Settings.json"), Settings);
        };
        InitializeComponent();
    }

    void PasswordChangedHandler(Object sender, RoutedEventArgs args)
    {
        Settings.AccessToken = PasswordBox.Password;

    }

    void PasswordBox_Loaded(object sender, RoutedEventArgs args)
    {
        PasswordBox = sender as PasswordBox;
        PasswordBox.Password = Settings.AccessToken;
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