namespace ThermalPrinterService.Services;

using ThermalPrinterService.Dtos;
using ThermalPrinterService.Models;

public class PrinterService
{
    private readonly PrinterState _printerState;
    private readonly PrinterJobService _printerJobService;
    private readonly PrinterHealthService _printerHealthService;
    private readonly PrinterLogService _printerLogService;

    public PrinterService(
        PrinterState printerState,
        PrinterJobService printerJobService,
        PrinterHealthService printerHealthService,
        PrinterLogService printerLogService)
    {
        _printerState = printerState;
        _printerJobService = printerJobService;
        _printerHealthService = printerHealthService;
        _printerLogService = printerLogService;
    }

    public void Connect(string mode)
    {
        _printerState.ConnectionMode = mode;
        _printerState.IsConnected = true;

        _printerLogService.AddLog("connect", "success");
    }

    public PrinterStatusResponseDto GetStatus()
    {
        return new PrinterStatusResponseDto
        {
            Service = "Thermal Printer Service",
            Connection = new ConnectionStatusDto
            {
                Connected = _printerState.IsConnected,
                Mode = _printerState.ConnectionMode
            },
            Health = _printerHealthService.GetHealthStatus(),
            LastJob = _printerJobService.GetLastJob(),
            Queue = _printerJobService.GetQueueSummary()
        };
    }

    public void PrintText(string text)
    {
        _printerJobService.PrintText(text);
    }

    public void PrintImage(string imageBase64)
    {
        _printerJobService.PrintImage(imageBase64);
    }

    public List<LogEntry> GetLogs()
    {
        return _printerLogService.GetLogs();
    }

    public void Reprint()
    {
        _printerJobService.Reprint();
    }

    public void SimulateError(string errorCode)
    {
        _printerState.CurrentErrorCode = errorCode;
        _printerLogService.AddLog("simulate_error", "success");
    }

    public void ClearError()
    {
        _printerState.CurrentErrorCode = null;
        _printerLogService.AddLog("clear_error", "success");
    }
}
