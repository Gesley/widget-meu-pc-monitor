using System.Text.Json;
using Microsoft.Web.WebView2.Wpf;
using PCMonitor.Models;
using PCMonitor.Services;

namespace PCMonitor.Interop;

public sealed class WebViewBridge
{
    private readonly WebView2 _webView;
    private readonly AppSettings _settings;
    private readonly HardwareMonitorService _monitor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals
    };

    public event Action? DragRequested;
    public event Action<AppSettings>? SettingsChanged;

    public WebViewBridge(WebView2 webView, AppSettings settings, HardwareMonitorService monitor)
    {
        _webView = webView;
        _settings = settings;
        _monitor = monitor;
    }

    public Task InitializeAsync()
    {
        _webView.CoreWebView2.WebMessageReceived += OnMessage;
        return Task.CompletedTask;
    }

    public void SendHardware(HardwareStatus status) =>
        Post(new BridgeMessage("hardware", status));

    public void SendSettings(AppSettings settings) =>
        Post(new BridgeMessage("settings", settings));

    public void SendGpus(IReadOnlyList<GpuOption> gpus) =>
        Post(new BridgeMessage("gpus", gpus));

    public void SendOpenSettings() =>
        Post(new BridgeMessage("openSettings", true));

    private void OnMessage(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs e)
    {
        try
        {
            var json = e.WebMessageAsJson;
            var envelope = JsonSerializer.Deserialize<BridgeMessage>(json, JsonOptions);
            if (envelope is null)
            {
                return;
            }

            switch (envelope.Type)
            {
                case "ready":
                    SendSettings(_settings);
                    SendHardware(_monitor.LastSnapshot);
                    SendGpus(_monitor.GetGpuOptions());
                    break;
                case "drag":
                    DragRequested?.Invoke();
                    break;
                case "settings":
                    var incoming = envelope.Payload.Deserialize<AppSettings>(JsonOptions);
                    if (incoming is not null)
                    {
                        Merge(incoming);
                        SettingsChanged?.Invoke(_settings);
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            AppLog.Error("Falha ao processar mensagem do WebView.", ex);
        }
    }

    private void Merge(AppSettings incoming)
    {
        _settings.Opacity = incoming.Opacity;
        _settings.AlwaysOnTop = incoming.AlwaysOnTop;
        _settings.StartWithWindows = incoming.StartWithWindows;
        _settings.ShowCharts = incoming.ShowCharts;
        _settings.ShowClock = incoming.ShowClock;
        _settings.ShowRam = incoming.ShowRam;
        _settings.PollIntervalMs = incoming.PollIntervalMs;
        _settings.Layout = incoming.Layout;
        _settings.Theme = incoming.Theme;
        _settings.InteractionMode = incoming.InteractionMode;
        _settings.SelectedGpuId = incoming.SelectedGpuId;
        _settings.TempWarn = incoming.TempWarn;
        _settings.TempAlert = incoming.TempAlert;
        _settings.TempCritical = incoming.TempCritical;
        _settings.UsageWarn = incoming.UsageWarn;
        _settings.UsageHigh = incoming.UsageHigh;
    }

    private void Post(BridgeMessage message)
    {
        if (_webView.CoreWebView2 is null)
        {
            return;
        }

        _webView.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(message, JsonOptions));
    }

    private sealed class BridgeMessage
    {
        public BridgeMessage() { }

        public BridgeMessage(string type, object payload)
        {
            Type = type;
            Payload = JsonSerializer.SerializeToElement(payload, JsonOptions);
        }

        public string Type { get; set; } = "";
        public JsonElement Payload { get; set; }
    }
}
