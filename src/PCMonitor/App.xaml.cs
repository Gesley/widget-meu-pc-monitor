using System.Windows;
using PCMonitor.Services;

namespace PCMonitor;

public partial class App : System.Windows.Application
{
    private TrayService? _tray;
    private HardwareMonitorService? _monitor;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        DispatcherUnhandledException += (_, args) =>
        {
            AppLog.Error("Exceção não tratada na UI.", args.Exception);
            args.Handled = true;
        };

        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            AppLog.Error("Exceção não tratada no domínio.", args.ExceptionObject as Exception);
        };

        try
        {
            var settings = SettingsService.Load();
            _monitor = new HardwareMonitorService(settings);
            _monitor.Start();

            _mainWindow = new MainWindow(settings, _monitor);
            _tray = new TrayService(_mainWindow, settings);

            if (settings.StartWithWindows)
            {
                StartupService.SetEnabled(true);
            }

            _mainWindow.Show();
        }
        catch (Exception ex)
        {
            AppLog.Error("Falha ao iniciar o aplicativo.", ex);
            System.Windows.MessageBox.Show(
                "Não foi possível iniciar o PC Monitor. Consulte o log em %AppData%\\PCMonitor.",
                "PC Monitor",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _tray?.Dispose();
        _monitor?.Dispose();
        base.OnExit(e);
    }
}
