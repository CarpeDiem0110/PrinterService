namespace ThermalPrinterService.Services;
using ThermalPrinterService.Models;
public class PrinterService
{
    public string? ConnectionMode { get; private set; }

    private string? _lastPrintedText;
    public bool IsConnected { get; private set; }

   private readonly List<LogEntry> _logs = new();

    public void Connect(string mode)
    {
        ConnectionMode = mode;
        IsConnected = true;

        _logs.Add($"[{DateTime.Now}] Connected via {mode}");
    }

    public object GetStatus()
    {
        return new
        {
            service = "Thermal Printer Service",
            connected = IsConnected,
            mode = ConnectionMode
        };
    }

    public void PrintText(string text)
    {
        if (!IsConnected)
        {
            _logs.Add($"[{DateTime.Now}] ERROR - PrintText failed: Printer is not connected.");
            throw new InvalidOperationException("Printer is not connected.");
        }
         _lastPrintedText = text;
        Console.WriteLine($"Printing text: {text}");
        _logs.Add($"[{DateTime.Now}] Printed text: {text}");
    }



// helper method  
   private void AddLog(string operation, string status, string message, string? jobId = null)
{
    _logs.Add(new LogEntry
    {
        Timestamp = DateTime.Now,
        Operation = operation,
        Status = status,
        Message = message,
        ConnectionMode = ConnectionMode,
        JobId = jobId
    });
}

    public List<LogEntry> GetLogs()
    {
    return _logs;
    }



    public void Reprint()
{
    if (!IsConnected)
    {
        _logs.Add($"[{DateTime.Now}] ERROR - Reprint failed: Printer is not connected.");
        throw new InvalidOperationException("Printer is not connected.");
    }

    if (string.IsNullOrWhiteSpace(_lastPrintedText))
    {
        _logs.Add($"[{DateTime.Now}] ERROR - Reprint failed: No previous print found.");
        throw new InvalidOperationException("No previous print found.");
    }

    Console.WriteLine($"Reprinting text: {_lastPrintedText}");

    _logs.Add($"[{DateTime.Now}] Reprinted text: {_lastPrintedText}");
}


}
