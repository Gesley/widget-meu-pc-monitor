using LibreHardwareMonitor.Hardware;
using PCMonitor.Models;

namespace PCMonitor.Services;

public sealed class HardwareMonitorService : IDisposable
{
    private const int HistoryLength = 60;

    private readonly SensorDiscoveryService _discovery = new();
    private readonly Computer _computer;
    private readonly UpdateVisitor _visitor = new();
    private readonly object _sync = new();
    private readonly List<double?> _cpuTempHistory = [];
    private readonly List<double?> _cpuUsageHistory = [];
    private readonly List<double?> _gpuTempHistory = [];
    private readonly List<double?> _gpuUsageHistory = [];
    private readonly List<double?> _ramUsageHistory = [];

    private AppSettings _settings;
    private CancellationTokenSource? _cts;
    private Task? _loop;
    private bool _opened;
    private bool _sensorsLogged;

    public HardwareMonitorService(AppSettings settings)
    {
        _settings = settings;
        _computer = new Computer
        {
            IsCpuEnabled = true,
            IsGpuEnabled = true,
            IsMemoryEnabled = true,
            IsMotherboardEnabled = true,
            IsControllerEnabled = false,
            IsNetworkEnabled = false,
            IsStorageEnabled = false
        };
    }

    public HardwareStatus LastSnapshot { get; private set; } = new();

    public event Action<HardwareStatus>? SnapshotReady;

    public void Start()
    {
        try
        {
            _computer.Open();
            _opened = true;
        }
        catch (Exception ex)
        {
            AppLog.Error("Não foi possível abrir o LibreHardwareMonitor. Sensores podem ficar indisponíveis.", ex);
        }

        _cts = new CancellationTokenSource();
        _loop = Task.Run(() => RunAsync(_cts.Token));
    }

    public void ApplySettings(AppSettings settings)
    {
        lock (_sync)
        {
            _settings = settings;
        }
    }

    public IReadOnlyList<GpuOption> GetGpuOptions()
    {
        if (!_opened)
        {
            return [];
        }

        try
        {
            _computer.Accept(_visitor);
            return _discovery.ToGpuOptions(_discovery.FindGpus(_computer));
        }
        catch (Exception ex)
        {
            AppLog.Error("Falha ao listar GPUs.", ex);
            return [];
        }
    }

    private async Task RunAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            int delay;
            lock (_sync)
            {
                delay = Math.Clamp(_settings.PollIntervalMs, 500, 5000);
            }

            try
            {
                var snapshot = Collect();
                LastSnapshot = snapshot;
                SnapshotReady?.Invoke(snapshot);
            }
            catch (Exception ex)
            {
                AppLog.Error("Falha ao coletar sensores.", ex);
            }

            try
            {
                await Task.Delay(delay, token);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private HardwareStatus Collect()
    {
        if (_opened)
        {
            _computer.Accept(_visitor);
            if (!_sensorsLogged)
            {
                _discovery.LogOnce(_computer);
                _sensorsLogged = true;
            }
        }

        string? gpuId;
        lock (_sync)
        {
            gpuId = _settings.SelectedGpuId;
        }

        var cpuHw = _opened ? _discovery.FindCpu(_computer) : null;
        var gpus = _opened ? _discovery.FindGpus(_computer) : [];
        var gpuHw = _discovery.SelectGpu(gpus, gpuId);
        var ramHw = _opened ? _discovery.FindMemory(_computer) : null;

        var cpu = new CpuStatus
        {
            Name = cpuHw?.Name ?? "CPU não detectada",
            Temperature = !_opened ? null : ReadTemp(_discovery.FindCpuTemperature(_computer, cpuHw)),
            Usage = cpuHw is null ? null : Read(_discovery.FindCpuLoad(cpuHw)),
            Clock = cpuHw is null ? null : ReadClock(_discovery.FindCpuClock(cpuHw))
        };

        var gpu = new GpuStatus
        {
            Name = gpuHw?.Name ?? "GPU não detectada",
            Identifier = gpuHw?.Identifier.ToString(),
            Temperature = gpuHw is null ? null : ReadTemp(_discovery.FindGpuTemperature(gpuHw)),
            Usage = gpuHw is null ? null : Read(_discovery.FindGpuLoad(gpuHw)),
            CoreClock = gpuHw is null ? null : Read(_discovery.FindGpuCoreClock(gpuHw)),
            MemoryClock = gpuHw is null ? null : Read(_discovery.FindGpuMemoryClock(gpuHw))
        };

        var used = ramHw is null ? null : Read(_discovery.FindRamUsed(ramHw));
        var available = ramHw is null ? null : Read(_discovery.FindRamAvailable(ramHw));
        var usage = ramHw is null ? null : Read(_discovery.FindRamLoad(ramHw));
        double? total = used is not null && available is not null ? used + available : null;
        if (usage is null && used is not null && total is > 0)
        {
            usage = used / total * 100.0;
        }

        var ram = new RamStatus
        {
            Used = used,
            Available = available,
            Total = total,
            Usage = usage,
            Temperature = _opened ? Read(_discovery.FindRamTemperature(_computer)) : null
        };

        Push(_cpuTempHistory, cpu.Temperature);
        Push(_cpuUsageHistory, cpu.Usage);
        Push(_gpuTempHistory, gpu.Temperature);
        Push(_gpuUsageHistory, gpu.Usage);
        Push(_ramUsageHistory, ram.Usage);

        cpu.TemperatureHistory = [.. _cpuTempHistory];
        cpu.UsageHistory = [.. _cpuUsageHistory];
        gpu.TemperatureHistory = [.. _gpuTempHistory];
        gpu.UsageHistory = [.. _gpuUsageHistory];
        ram.UsageHistory = [.. _ramUsageHistory];

        return new HardwareStatus
        {
            Cpu = cpu,
            Gpu = gpu,
            Ram = ram,
            Timestamp = DateTime.UtcNow
        };
    }

    private static double? Read(ISensor? sensor)
    {
        if (sensor?.Value is not float value || float.IsNaN(value) || float.IsInfinity(value))
        {
            return null;
        }

        return Math.Round(value, 1);
    }

    private static double? ReadTemp(ISensor? sensor)
    {
        var value = Read(sensor);
        return value is > 1 and < 125 ? value : null;
    }

    private static double? ReadClock(ISensor? sensor)
    {
        var value = Read(sensor);
        return value is > 100 ? value : null;
    }

    private static void Push(List<double?> history, double? value)
    {
        history.Add(value);
        if (history.Count > HistoryLength)
        {
            history.RemoveRange(0, history.Count - HistoryLength);
        }
    }

    public void Dispose()
    {
        try
        {
            _cts?.Cancel();
            _loop?.Wait(TimeSpan.FromSeconds(2));
        }
        catch (Exception ex)
        {
            AppLog.Error("Falha ao encerrar o loop de monitoramento.", ex);
        }

        try
        {
            if (_opened)
            {
                _computer.Close();
            }
        }
        catch (Exception ex)
        {
            AppLog.Error("Falha ao fechar o LibreHardwareMonitor.", ex);
        }

        _cts?.Dispose();
    }
}
