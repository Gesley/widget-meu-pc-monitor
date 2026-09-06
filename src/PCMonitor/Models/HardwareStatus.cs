namespace PCMonitor.Models;

public sealed class HardwareStatus
{
    public CpuStatus Cpu { get; set; } = new();
    public GpuStatus Gpu { get; set; } = new();
    public RamStatus Ram { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public sealed class CpuStatus
{
    public string Name { get; set; } = "CPU não detectada";
    public double? Temperature { get; set; }
    public double? Usage { get; set; }
    public double? Clock { get; set; }
    public List<double?> TemperatureHistory { get; set; } = [];
    public List<double?> UsageHistory { get; set; } = [];
}

public sealed class GpuStatus
{
    public string Name { get; set; } = "GPU não detectada";
    public string? Identifier { get; set; }
    public double? Temperature { get; set; }
    public double? Usage { get; set; }
    public double? CoreClock { get; set; }
    public double? MemoryClock { get; set; }
    public List<double?> TemperatureHistory { get; set; } = [];
    public List<double?> UsageHistory { get; set; } = [];
}

public sealed class RamStatus
{
    public double? Used { get; set; }
    public double? Available { get; set; }
    public double? Total { get; set; }
    public double? Usage { get; set; }
    public double? Temperature { get; set; }
    public List<double?> UsageHistory { get; set; } = [];
}

public sealed class GpuOption
{
    public string Identifier { get; set; } = "";
    public string Name { get; set; } = "";
}
