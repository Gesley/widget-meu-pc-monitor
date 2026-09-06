using System.Diagnostics;

namespace PCMonitor.Services;

public static class CpuClockFallback
{
    public static double? ReadMhz()
    {
        try
        {
            using var counter = new PerformanceCounter("Processor Information", "Processor Frequency", "_Total");
            counter.NextValue();
            var value = counter.NextValue();
            return value > 100 ? Math.Round(value, 0) : null;
        }
        catch (Exception ex)
        {
            AppLog.Error("Fallback de clock da CPU indisponível.", ex);
            return null;
        }
    }
}
