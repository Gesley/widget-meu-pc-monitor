using LibreHardwareMonitor.Hardware;
using PCMonitor.Models;

namespace PCMonitor.Services;

public sealed class UpdateVisitor : IVisitor
{
    public void VisitComputer(IComputer computer) => computer.Traverse(this);

    public void VisitHardware(IHardware hardware)
    {
        hardware.Update();
        foreach (var sub in hardware.SubHardware)
        {
            sub.Accept(this);
        }
    }

    public void VisitSensor(ISensor sensor) { }

    public void VisitParameter(IParameter parameter) { }
}

public sealed class SensorDiscoveryService
{
    public IHardware? FindCpu(IComputer computer) =>
        computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Cpu);

    public IReadOnlyList<IHardware> FindGpus(IComputer computer) =>
        computer.Hardware
            .Where(h => h.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd or HardwareType.GpuIntel)
            .ToList();

    public IHardware? SelectGpu(IReadOnlyList<IHardware> gpus, string? selectedId)
    {
        if (gpus.Count == 0)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(selectedId))
        {
            var match = gpus.FirstOrDefault(g => g.Identifier.ToString() == selectedId);
            if (match is not null)
            {
                return match;
            }
        }

        return gpus.FirstOrDefault(g => g.HardwareType is HardwareType.GpuNvidia or HardwareType.GpuAmd)
               ?? gpus[0];
    }

    public IHardware? FindMemory(IComputer computer) =>
        computer.Hardware.FirstOrDefault(h => h.HardwareType == HardwareType.Memory);

    public ISensor? FindCpuTemperature(IComputer computer, IHardware? cpu)
    {
        ISensor? fromCpu = cpu is null
            ? null
            : Pick(cpu, SensorType.Temperature, ["tctl", "tdie", "package", "ccd", "die", "average", "core"], requirePositive: true);

        if (HasTemp(fromCpu))
        {
            return fromCpu;
        }

        foreach (var board in computer.Hardware.Where(h => h.HardwareType == HardwareType.Motherboard))
        {
            var fromBoard = Pick(
                board,
                SensorType.Temperature,
                ["tctl", "cpu", "package"],
                exclude: ["gpu", "vrm", "pch", "chipset", "motherboard", "system", "ambient"],
                requirePositive: true);
            if (HasTemp(fromBoard))
            {
                return fromBoard;
            }
        }

        return fromCpu;
    }

    public ISensor? FindCpuLoad(IHardware cpu) =>
        Pick(cpu, SensorType.Load, ["total", "cpu total"]);

    public ISensor? FindCpuClock(IHardware cpu)
    {
        var sensors = EnumerateSensors(cpu)
            .Where(s => s.SensorType == SensorType.Clock)
            .Where(s => !s.Name.Contains("bus", StringComparison.OrdinalIgnoreCase))
            .Where(s => s.Value is > 100)
            .ToList();

        var preferred = Pick(cpu, SensorType.Clock, ["core #1", "core #0", "cpu core", "core"], exclude: ["bus"]);
        if (preferred?.Value is > 100)
        {
            return preferred;
        }

        return sensors.MaxBy(s => s.Value);
    }

    public ISensor? FindGpuTemperature(IHardware gpu) =>
        Pick(gpu, SensorType.Temperature, ["core", "hot spot", "gpu"]);

    public ISensor? FindGpuLoad(IHardware gpu) =>
        Pick(gpu, SensorType.Load, ["core", "d3d 3d", "gpu"]);

    public ISensor? FindGpuCoreClock(IHardware gpu) =>
        Pick(gpu, SensorType.Clock, ["core"], exclude: ["memory", "bus"]);

    public ISensor? FindGpuMemoryClock(IHardware gpu) =>
        Pick(gpu, SensorType.Clock, ["memory"]);

    public ISensor? FindRamLoad(IHardware? memory) =>
        memory is null ? null : Pick(memory, SensorType.Load, ["memory"]);

    public ISensor? FindRamUsed(IHardware? memory) =>
        memory is null ? null : Pick(memory, SensorType.Data, ["memory used", "used"]);

    public ISensor? FindRamAvailable(IHardware? memory) =>
        memory is null ? null : Pick(memory, SensorType.Data, ["memory available", "available"]);

    public ISensor? FindRamTemperature(IComputer computer)
    {
        foreach (var hardware in computer.Hardware)
        {
            var sensors = EnumerateSensors(hardware)
                .Where(s => s.SensorType == SensorType.Temperature);
            foreach (var sensor in sensors)
            {
                var name = sensor.Name.ToLowerInvariant();
                if (name.Contains("dimm") || name.Contains("dram") || name.Contains("spd"))
                {
                    return sensor;
                }
            }
        }

        return null;
    }

    public IReadOnlyList<GpuOption> ToGpuOptions(IEnumerable<IHardware> gpus) =>
        gpus.Select(g => new GpuOption
        {
            Identifier = g.Identifier.ToString(),
            Name = g.Name
        }).ToList();

    public void LogOnce(IComputer computer)
    {
        try
        {
            var lines = computer.Hardware.Select(h =>
            {
                var sensors = string.Join(", ", EnumerateSensors(h).Select(s => $"{s.Name}={s.Value} ({s.SensorType})"));
                return $"{h.HardwareType}:{h.Name} [{sensors}]";
            });
            AppLog.Info("Sensores detectados: " + string.Join(" | ", lines));
        }
        catch (Exception ex)
        {
            AppLog.Error("Falha ao registrar sensores.", ex);
        }
    }

    private static ISensor? Pick(
        IHardware hardware,
        SensorType type,
        string[] preferred,
        string[]? exclude = null,
        bool requirePositive = false)
    {
        var sensors = EnumerateSensors(hardware)
            .Where(s => s.SensorType == type)
            .ToList();

        if (exclude is { Length: > 0 })
        {
            sensors = sensors
                .Where(s => exclude.All(x => s.Name.Contains(x, StringComparison.OrdinalIgnoreCase) == false))
                .ToList();
        }

        if (requirePositive)
        {
            sensors = sensors.Where(HasTemp).ToList();
        }

        foreach (var token in preferred)
        {
            var match = sensors.FirstOrDefault(s => s.Name.Contains(token, StringComparison.OrdinalIgnoreCase));
            if (match is not null)
            {
                return match;
            }
        }

        return sensors.FirstOrDefault();
    }

    private static bool HasTemp(ISensor? sensor) =>
        sensor?.Value is > 1 and < 125;

    private static IEnumerable<ISensor> EnumerateSensors(IHardware hardware)
    {
        foreach (var sensor in hardware.Sensors)
        {
            yield return sensor;
        }

        foreach (var sub in hardware.SubHardware)
        {
            foreach (var sensor in EnumerateSensors(sub))
            {
                yield return sensor;
            }
        }
    }
}
