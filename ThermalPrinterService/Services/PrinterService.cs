namespace ThermalPrinterService.Services;
using ThermalPrinterService.Models;
public class PrinterService
{
    public string? ConnectionMode { get; private set; }
    private string? _lastPrintedText;
    public bool IsConnected { get; private set; }
    private readonly List<LogEntry> _logs = new();
    private string? _currentErrorCode;

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
        
        var jobId = Guid.NewGuid().ToString();
        
        CheckPrinterState("print_text",jobId);
        _lastPrintedText = text;
        
        AddLog("print_text", "success", $"Printed text: {text}",jobId);
    }


public void PrintImage(string imageBase64)
{
   
    var jobId = Guid.NewGuid().ToString();
   
    CheckPrinterState("print_image", jobId);

    if (string.IsNullOrWhiteSpace(imageBase64))
    {
        AddLog("print_image", "error", "Image data is empty.");
        throw new InvalidOperationException("Image data is empty.");
    }

   

    AddLog("print_image", "success", "Image printed successfully.",jobId);
}


// HELPER METHODS

// add log helper method 
private void AddLog(
    string operation,
    string status,
    string? jobId = null,
    LogError? error = null)
{
    _logs.Add(new LogEntry
    {
        Timestamp = DateTime.Now,
        Operation = operation,
        Status = status,
        ConnectionMode = ConnectionMode,
        JobId = jobId,
        Error = error
    });
}

// Fınd and understand error code detail helper method
private string GetErrorDetail(string errorCode)
{
    return errorCode switch
    {
        PrinterErrorCodes.PAPER_OUT => "No paper detected.",
        PrinterErrorCodes.PAPER_JAM => "Paper jam detected.",
        PrinterErrorCodes.COVER_OPEN => "Printer cover is open.",
        PrinterErrorCodes.OVERHEAT => "Printer is overheated.",
        PrinterErrorCodes.COMM_ERROR => "Printer communication failed.",
        PrinterErrorCodes.UNKNOWN_COMMAND => "Unknown printer command.",
        _ => "Unknown printer error."
    };
}



// Check if printer is not connected and its error code helper method 
private void CheckPrinterState(string operation, string jobId)
{
    if (!IsConnected)
    {
        AddLog(
            operation,
            "error",
            GetErrorDetail(PrinterErrorCodes.COMM_ERROR),
            jobId,
            new LogError
            {
                Code = PrinterErrorCodes.COMM_ERROR,
                Detail = GetErrorDetail(PrinterErrorCodes.COMM_ERROR)
            }
        );

        throw new InvalidOperationException(GetErrorDetail(PrinterErrorCodes.COMM_ERROR));
    }

    if (_currentErrorCode != null)
    {
        AddLog(
            operation,
            "error",
            GetErrorDetail(_currentErrorCode),
            jobId,
            new LogError
            {
                Code = _currentErrorCode,
                Detail = GetErrorDetail(_currentErrorCode)
            }
        );

        throw new InvalidOperationException(GetErrorDetail(_currentErrorCode));
    }
}

    public List<LogEntry> GetLogs()
    {
    return _logs;
    }



    public void Reprint()
{
    var jobId = Guid.NewGuid().ToString();

    CheckPrinterState("reprint", jobId);

    if (string.IsNullOrWhiteSpace(_lastPrintedText))
    {
        AddLog("reprint", "error", "No previous print found.", jobId);
        throw new InvalidOperationException("No previous print found.");
    }

    Console.WriteLine($"Reprinting text: {_lastPrintedText}");

    AddLog("reprint", "success", $"Reprinted text: {_lastPrintedText}", jobId);
}



public void SimulateError(string errorCode)
{
    _currentErrorCode = errorCode;

    AddLog(
        "simulate_error",
        "success",
        $"Simulated error: {errorCode}"
    );
}


 public void ClearError()
    {
    _currentErrorCode = null;

    AddLog(
        "clear_error",
        "success",
        "Printer error cleared."
    );
}





}
