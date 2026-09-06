namespace PCMonitor.Models;

public sealed class AppSettings
{
    public double? Left { get; set; }
    public double? Top { get; set; }
    public double Width { get; set; } = 420;
    public double Height { get; set; } = 560;
    public double Opacity { get; set; } = 0.92;
    public bool AlwaysOnTop { get; set; } = true;
    public bool StartWithWindows { get; set; }
    public bool ShowCharts { get; set; } = true;
    public bool ShowClock { get; set; } = true;
    public bool ShowRam { get; set; } = true;
    public int PollIntervalMs { get; set; } = 1000;
    public string Layout { get; set; } = "normal";
    public string Theme { get; set; } = "dark";
    public bool InteractionMode { get; set; } = true;
    public string? SelectedGpuId { get; set; }
    public string? MonitorDeviceName { get; set; }
    public double TempWarn { get; set; } = 60;
    public double TempAlert { get; set; } = 80;
    public double TempCritical { get; set; } = 90;
    public double UsageWarn { get; set; } = 50;
    public double UsageHigh { get; set; } = 80;
}
