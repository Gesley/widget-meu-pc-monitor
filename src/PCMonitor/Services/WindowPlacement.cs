using System.Windows;
using System.Windows.Forms;
using PCMonitor.Models;

namespace PCMonitor.Services;

public static class WindowPlacement
{
    public static bool TryRestore(AppSettings settings, Window window)
    {
        if (settings.Left is null || settings.Top is null)
        {
            return false;
        }

        var bounds = new System.Drawing.Rectangle(
            (int)Math.Round(settings.Left.Value),
            (int)Math.Round(settings.Top.Value),
            (int)Math.Max(1, Math.Round(settings.Width)),
            (int)Math.Max(1, Math.Round(settings.Height)));

        var screens = Screen.AllScreens;
        var onAny = screens.Any(s => s.WorkingArea.IntersectsWith(bounds));
        if (!onAny)
        {
            var preferred = screens.FirstOrDefault(s => s.DeviceName == settings.MonitorDeviceName)
                            ?? Screen.PrimaryScreen;
            if (preferred is null)
            {
                return false;
            }

            window.Left = preferred.WorkingArea.Left + 24;
            window.Top = preferred.WorkingArea.Top + 24;
            return true;
        }

        window.Left = settings.Left.Value;
        window.Top = settings.Top.Value;
        ClampToVisible(window);
        return true;
    }

    public static void ClampToVisible(Window window)
    {
        var rect = new System.Drawing.Rectangle(
            (int)Math.Round(window.Left),
            (int)Math.Round(window.Top),
            (int)Math.Max(1, Math.Round(window.Width)),
            (int)Math.Max(1, Math.Round(window.Height)));

        if (Screen.AllScreens.Any(s => s.WorkingArea.IntersectsWith(rect)))
        {
            return;
        }

        var primary = Screen.PrimaryScreen?.WorkingArea ?? new System.Drawing.Rectangle(0, 0, 1920, 1080);
        window.Left = primary.Left + 24;
        window.Top = primary.Top + 24;
    }

    public static string? GetCurrentMonitorName(Window window)
    {
        var point = new System.Drawing.Point((int)window.Left, (int)window.Top);
        return Screen.FromPoint(point).DeviceName;
    }
}
