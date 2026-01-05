using System.IO;
using System.Windows;
using Microsoft.Web.WebView2.Core;

namespace DepictPlots;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// https://github.com/pa7/heatmap.js#
/// </summary>
public partial class MainWindow : Window
{
    private ScriptBridge StringBridge { get; set; }

    public MainWindow()
    {
        InitializeComponent();
        InitializeAsync();
    }
        
    private async void InitializeAsync()
    {
        await WebView.EnsureCoreWebView2Async(null);
        StringBridge = new ScriptBridge(WebView.CoreWebView2);
        WebView.CoreWebView2.Settings.IsScriptEnabled = true;
        WebView.CoreWebView2.AddHostObjectToScript("bridge", StringBridge);
        // 获取当前可执行文件的目录
        var currentDirectory = Directory.GetCurrentDirectory();

        // 构建 HTML 文件的完整路径
        var htmlFilePath = Path.Combine(currentDirectory, "examples", "heatmap-legend", "index.html");

        // 设置 WebView2 的 Source 属性
        WebView.Source = new Uri(htmlFilePath);

        // 注册 WebMessageReceived 事件
        WebView.CoreWebView2.WebMessageReceived += OnWebMessageReceived;
        // 注册 JavaScript 到 C# 的回调
        //WebView.CoreWebView2.AddHostObjectToScript("bridge", new ScriptBridge(WebView.CoreWebView2));
    }

    private static void OnWebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs args)
    {
        // 处理从 JavaScript 发送的消息
        var message = args.TryGetWebMessageAsString();
        MessageBox.Show($"Image clicked: {message}");
    }
    
    public class ScriptBridge(CoreWebView2 coreWebView2)
    {
        public void CallFromJs(string message)
        {
            // 在这里处理从 JavaScript 传递过来的消息
            // 使用 CoreWebView2 的 PostWebMessageAsString 方法发送消息
            coreWebView2.PostWebMessageAsString(message);
        }
    }
}