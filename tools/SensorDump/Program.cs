using LibreHardwareMonitor.Hardware;

var computer = new Computer
{
    IsCpuEnabled = true,
    IsGpuEnabled = false,
    IsMemoryEnabled = false,
    IsMotherboardEnabled = true
};
computer.Open();

var visitor = new UpdateVisitor();
computer.Accept(visitor);
Thread.Sleep(500);
computer.Accept(visitor);

foreach (var hw in computer.Hardware)
{
    Console.WriteLine($"== {hw.HardwareType}: {hw.Name}");
    Dump(hw, "  ");
}

computer.Close();

static void Dump(IHardware hw, string indent)
{
    foreach (var s in hw.Sensors.Where(s => s.SensorType is SensorType.Temperature or SensorType.Clock))
    {
        Console.WriteLine($"{indent}{s.Name} = {s.Value} ({s.SensorType})");
    }

    foreach (var sub in hw.SubHardware)
    {
        Dump(sub, indent + "  ");
    }
}

sealed class UpdateVisitor : IVisitor
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
