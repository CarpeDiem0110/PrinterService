namespace ThermalPrinterService.Models;

public class LogEntry
{
    public DateTime Timestamp { get; set; } = DateTime.Now;

    public string Operation { get; set; } = "";

    public string Status { get; set; } = "";

    public string? ConnectionMode { get; set; }

    public string? JobId { get; set; }

    public LogError? Error { get; set; }

}
