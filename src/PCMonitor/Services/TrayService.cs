using System.Windows;
using System.Windows.Forms;
using PCMonitor.Models;

namespace PCMonitor.Services;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _icon;
    private readonly MainWindow _window;
    private readonly AppSettings _settings;

    public TrayService(MainWindow window, AppSettings settings)
    {
        _window = window;
        _settings = settings;

        _icon = new NotifyIcon
        {
            Text = "PC Monitor",
            Visible = true,
            Icon = System.Drawing.SystemIcons.Application
        };

        _icon.ContextMenuStrip = BuildMenu();
        _icon.DoubleClick += (_, _) => _window.Dispatcher.Invoke(_window.ToggleWidget);
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Mostrar widget", null, (_, _) => _window.Dispatcher.Invoke(_window.ShowWidget));
        menu.Items.Add("Ocultar widget", null, (_, _) => _window.Dispatcher.Invoke(_window.HideWidget));
        menu.Items.Add("Configurações", null, (_, _) => _window.Dispatcher.Invoke(() =>
        {
            _window.ShowWidget();
            _window.OpenSettings();
        }));
        menu.Items.Add(new ToolStripSeparator());

        var startItem = new ToolStripMenuItem("Iniciar com Windows")
        {
            Checked = _settings.StartWithWindows,
            CheckOnClick = true
        };
        startItem.CheckedChanged += (_, _) =>
        {
            _settings.StartWithWindows = startItem.Checked;
            StartupService.SetEnabled(startItem.Checked);
            SettingsService.Save(_settings);
        };
        menu.Items.Add(startItem);

        var topItem = new ToolStripMenuItem("Sempre no topo")
        {
            Checked = _settings.AlwaysOnTop,
            CheckOnClick = true
        };
        topItem.CheckedChanged += (_, _) =>
        {
            _settings.AlwaysOnTop = topItem.Checked;
            SettingsService.Save(_settings);
            _window.Dispatcher.Invoke(_window.ApplySettingsToWindow);
        };
        menu.Items.Add(topItem);

        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Sair", null, (_, _) => _window.Dispatcher.Invoke(_window.ExitApp));
        return menu;
    }

    public void Dispose()
    {
        _icon.Visible = false;
        _icon.Dispose();
    }
}
