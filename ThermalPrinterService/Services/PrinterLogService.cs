namespace ThermalPrinterService.Services;

using ThermalPrinterService.Models;

public class PrinterLogService
{
    private readonly PrinterState _printerState;
    private readonly List<LogEntry> _logs = new();

    public PrinterLogService(PrinterState printerState)
    {
        _printerState = printerState;
    }

    public List<LogEntry> GetLogs()
    {
        return _logs;
    }

    public void AddLog(
        string operation,
        string status,
        string? jobId = null,
        IEnumerable<LogError>? errors = null)
    {
        _logs.Add(new LogEntry
        {
            Timestamp = DateTime.Now,
            Operation = operation,
            Status = status,
            ConnectionMode = _printerState.ConnectionMode,
            JobId = jobId,
            Errors = errors?.ToList() ?? new List<LogError>()
        });
    }
}
