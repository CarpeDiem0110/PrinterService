namespace ThermalPrinterService.Models;

public class PrinterState
{
    public string? ConnectionMode { get; set; }

    public bool IsConnected { get; set; }

    public List<string> CurrentErrorCodes { get; } = new();
}
