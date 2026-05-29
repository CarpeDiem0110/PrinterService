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

       AddLog("connect", "success", $"Connected via {mode}");
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
            AddLog("print_text", "error", "Printer is not connected.");
            throw new InvalidOperationException("Printer is not connected.");
        }
         _lastPrintedText = text;
        
        var jobId = Guid.NewGuid().ToString();

        AddLog("print_text", "success", $"Printed text: {text}",jobId);
    }



// helper method for adding log 
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
        AddLog("reprint", "error", "Printer is not connected.");
        throw new InvalidOperationException("Printer is not connected.");
    }

    if (string.IsNullOrWhiteSpace(_lastPrintedText))
    {
        AddLog("reprint", "error", "No previous print found.");
        throw new InvalidOperationException("No previous print found.");
    }

    Console.WriteLine($"Reprinting text: {_lastPrintedText}");

    AddLog("reprint", "success", $"Reprinted text: {_lastPrintedText}");
}


}
