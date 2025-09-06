using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClassIsland.Shared.Helpers;
using GSVIsland.Models;
using GSVIsland.Shared;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

namespace GSVIsland.Controls.SpeechProviderSettingsControls;

/// <summary>
/// GSVISpeechServiceSettingsControl.xaml 的交互逻辑
/// </summary>
public partial class GSVISpeechServiceSettingsControl : INotifyPropertyChanged
{
    public GSVISpeechSettings Settings { get; set; } = new();
    
    // 在 Avalonia 中，我们使用标准的 TextBox 来处理密码
    private TextBox _passwordTextBox = new();

    public GSVISpeechServiceSettingsControl()
    {
        InitializeComponent();
        
        // 在 Avalonia 中，Loaded 事件的处理方式不同
        this.AttachedToVisualTree += OnAttachedToVisualTree;
        
        LoadSettings();
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        // 控件已附加到视觉树，可以初始化设置
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
        
        // 设置密码框的初始值（如果已经创建）
        if (_passwordTextBox != null)
        {
            _passwordTextBox.Text = Settings.AccessToken;
        }
        
        // 设置数据上下文
        this.DataContext = this;
    }

    // 密码变更处理（在 XAML 中绑定到 TextBox 的 TextChanged 事件）
    private void PasswordChangedHandler(object sender, TextChangedEventArgs args)
    {
        if (sender is TextBox textBox)
        {
            Settings.AccessToken = textBox.Text;
        }
    }

    // 密码框加载处理（在 XAML 中绑定到 TextBox 的 AttachedToVisualTree 事件）
    private void PasswordBox_Loaded(object sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            _passwordTextBox = textBox;
            textBox.Text = Settings.AccessToken;
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
