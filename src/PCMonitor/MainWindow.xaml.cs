using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Web.WebView2.Core;
using PCMonitor.Interop;
using PCMonitor.Models;
using PCMonitor.Services;

namespace PCMonitor;

public partial class MainWindow : Window
{
    private readonly AppSettings _settings;
    private readonly HardwareMonitorService _monitor;
    private WebViewBridge? _bridge;
    private bool _clickThrough;

    public MainWindow(AppSettings settings, HardwareMonitorService monitor)
    {
        InitializeComponent();
        _settings = settings;
        _monitor = monitor;

        Loaded += OnLoaded;
        Closing += OnClosing;
        LocationChanged += (_, _) => PersistBounds();
        SizeChanged += (_, _) => PersistBounds();
        SourceInitialized += OnSourceInitialized;
    }

    public void ApplySettingsToWindow()
    {
        Topmost = _settings.AlwaysOnTop;
        Opacity = Math.Clamp(_settings.Opacity, 0.4, 1.0);
        SetClickThrough(!_settings.InteractionMode);
    }

    public void ShowWidget()
    {
        Show();
        WindowState = WindowState.Normal;
        Activate();
    }

    public void HideWidget()
    {
        Hide();
    }

    public void ToggleWidget()
    {
        if (IsVisible) HideWidget();
        else ShowWidget();
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        RestoreWindowBounds();
        ApplySettingsToWindow();

        var env = await CoreWebView2Environment.CreateAsync();
        await WebView.EnsureCoreWebView2Async(env);

        WebView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
        WebView.CoreWebView2.Settings.AreDevToolsEnabled = true;
        WebView.CoreWebView2.Settings.IsStatusBarEnabled = false;
        WebView.CoreWebView2.Settings.AreBrowserAcceleratorKeysEnabled = true;

        var uiPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        Directory.CreateDirectory(uiPath);

        WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(
            "app.pcmonitor",
            uiPath,
            CoreWebView2HostResourceAccessKind.Allow);

        _bridge = new WebViewBridge(WebView, _settings, _monitor);
        _bridge.SettingsChanged += OnSettingsFromUi;
        await _bridge.InitializeAsync();

        _monitor.SnapshotReady += snapshot =>
        {
            Dispatcher.BeginInvoke(() => _bridge?.SendHardware(snapshot));
        };

        var index = Path.Combine(uiPath, "index.html");
        if (!File.Exists(index))
        {
            File.WriteAllText(index, FallbackHtml());
        }

        WebView.CoreWebView2.Navigate("https://app.pcmonitor/index.html");
        _bridge.SendSettings(_settings);
        _bridge.SendHardware(_monitor.LastSnapshot);
        _bridge.SendGpus(_monitor.GetGpuOptions());
    }

    public void OpenSettings() => _bridge?.SendOpenSettings();

    private void OnSettingsFromUi(AppSettings settings)
    {
        SettingsService.Save(settings);
        _monitor.ApplySettings(settings);
        ApplySettingsToWindow();
        StartupService.SetEnabled(settings.StartWithWindows);
        _bridge?.SendSettings(settings);
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = true;
        HideWidget();
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var hwnd = new WindowInteropHelper(this).Handle;
        var source = HwndSource.FromHwnd(hwnd);
        source?.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int wmNcHitTest = 0x0084;
        const int htLeft = 10;
        const int htRight = 11;
        const int htTop = 12;
        const int htTopLeft = 13;
        const int htTopRight = 14;
        const int htBottom = 15;
        const int htBottomLeft = 16;
        const int htBottomRight = 17;

        if (msg == wmNcHitTest)
        {
            if (!_settings.InteractionMode)
            {
                return IntPtr.Zero;
            }

            var mouse = PointFromScreen(new System.Windows.Point(
                (short)(lParam.ToInt32() & 0xFFFF),
                (short)((lParam.ToInt32() >> 16) & 0xFFFF)));

            const int grip = 8;
            bool left = mouse.X <= grip;
            bool right = mouse.X >= ActualWidth - grip;
            bool top = mouse.Y <= grip;
            bool bottom = mouse.Y >= ActualHeight - grip;

            if (top && left) { handled = true; return new IntPtr(htTopLeft); }
            if (top && right) { handled = true; return new IntPtr(htTopRight); }
            if (bottom && left) { handled = true; return new IntPtr(htBottomLeft); }
            if (bottom && right) { handled = true; return new IntPtr(htBottomRight); }
            if (left) { handled = true; return new IntPtr(htLeft); }
            if (right) { handled = true; return new IntPtr(htRight); }
            if (top) { handled = true; return new IntPtr(htTop); }
            if (bottom) { handled = true; return new IntPtr(htBottom); }

            handled = true;
            return new IntPtr(1);
        }

        return IntPtr.Zero;
    }

    private void OnHeaderMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ChangedButton != System.Windows.Input.MouseButton.Left || IsFromButton(e.OriginalSource))
        {
            return;
        }

        try
        {
            DragMove();
        }
        catch (Exception ex)
        {
            AppLog.Error("Falha ao arrastar a janela.", ex);
        }
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        OpenSettings();
    }

    private static bool IsFromButton(object? source)
    {
        var current = source as DependencyObject;
        while (current is not null)
        {
            if (current is System.Windows.Controls.Button)
            {
                return true;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return false;
    }

    private void RestoreWindowBounds()
    {
        Width = Math.Clamp(_settings.Width, MinWidth, MaxWidth);
        Height = Math.Clamp(_settings.Height, MinHeight, MaxHeight);

        if (WindowPlacement.TryRestore(_settings, this))
        {
            return;
        }

        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void PersistBounds()
    {
        if (!IsLoaded || WindowState != WindowState.Normal)
        {
            return;
        }

        _settings.Left = Left;
        _settings.Top = Top;
        _settings.Width = Width;
        _settings.Height = Height;
        _settings.MonitorDeviceName = WindowPlacement.GetCurrentMonitorName(this);
        SettingsService.Save(_settings);
    }

    private void SetClickThrough(bool enabled)
    {
        _clickThrough = enabled;
        var hwnd = new WindowInteropHelper(this).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        NativeWindow.SetClickThrough(hwnd, enabled);
    }

    private static string FallbackHtml() =>
        """
        <!doctype html>
        <html><body style="font-family:Segoe UI;color:#fff;background:rgba(12,12,16,.72);padding:24px">
        Interface Vue não encontrada. Execute <code>npm run build</code> em <code>frontend</code> e copie <code>dist</code> para <code>wwwroot</code>.
        </body></html>
        """;
}

internal static class NativeWindow
{
    private const int GwlExStyle = -20;
    private const int WsExTransparent = 0x00000020;
    private const int WsExLayered = 0x00080000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern bool ReleaseCapture();

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

    public static void BeginCaptionDrag(IntPtr hwnd)
    {
        const int wmNcLButtonDown = 0x00A1;
        const int htCaption = 0x0002;
        ReleaseCapture();
        SendMessage(hwnd, wmNcLButtonDown, new IntPtr(htCaption), IntPtr.Zero);
    }

    public static void SetClickThrough(IntPtr hwnd, bool enable)
    {
        var style = GetWindowLong(hwnd, GwlExStyle);
        style |= WsExLayered;
        if (enable)
        {
            style |= WsExTransparent;
        }
        else
        {
            style &= ~WsExTransparent;
        }

        SetWindowLong(hwnd, GwlExStyle, style);
    }
}
