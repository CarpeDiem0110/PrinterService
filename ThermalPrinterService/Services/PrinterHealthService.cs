namespace ThermalPrinterService.Services;

using Microsoft.AspNetCore.Http;
using ThermalPrinterService.Dtos;
using ThermalPrinterService.Exceptions;
using ThermalPrinterService.Models;

public class PrinterHealthService
{
    private readonly PrinterState _printerState;
    private readonly PrinterLogService _printerLogService;

    public PrinterHealthService(
        PrinterState printerState,
        PrinterLogService printerLogService)
    {
        _printerState = printerState;
        _printerLogService = printerLogService;
    }

    public PrinterHealthStatusDto GetHealthStatus()
    {
        var currentErrors = _printerState.CurrentErrorCodes
            .Select(errorCode => new PrinterErrorStatusDto
            {
                Code = errorCode,
                Detail = GetErrorDetail(errorCode)
            })
            .ToList();

        return new PrinterHealthStatusDto
        {
            Paper = GetPaperStatus(),
            Cover = GetCoverStatus(),
            Temperature = GetTemperatureStatus(),
            CurrentErrors = currentErrors
        };
    }

    public void CheckPrinterState(string operation, string jobId)
    {
        if (!_printerState.IsConnected)
        {
            AddErrorLog(operation, jobId, new[] { PrinterErrorCodes.COMM_ERROR });

            throw new ApiException(
                StatusCodes.Status409Conflict,
                GetErrorDetail(PrinterErrorCodes.COMM_ERROR));
        }

        if (_printerState.CurrentErrorCodes.Count > 0)
        {
            AddErrorLog(operation, jobId, _printerState.CurrentErrorCodes);

            throw new ApiException(
                StatusCodes.Status409Conflict,
                GetActiveErrorMessage());
        }
    }

    public string GetErrorDetail(string errorCode)
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

    private void AddErrorLog(
        string operation,
        string jobId,
        IEnumerable<string> errorCodes)
    {
        var errors = errorCodes
            .Select(errorCode => new LogError
            {
                Code = errorCode,
                Detail = GetErrorDetail(errorCode)
            })
            .ToList();

        _printerLogService.AddLog(
            operation,
            "error",
            jobId,
            errors
        );
    }

    private string GetPaperStatus()
    {
        if (_printerState.CurrentErrorCodes.Contains(PrinterErrorCodes.PAPER_OUT))
        {
            return "out";
        }

        if (_printerState.CurrentErrorCodes.Contains(PrinterErrorCodes.PAPER_JAM))
        {
            return "jammed";
        }

        return "ok";
    }

    private string GetCoverStatus()
    {
        return _printerState.CurrentErrorCodes.Contains(PrinterErrorCodes.COVER_OPEN) ? "open" : "closed";
    }

    private string GetTemperatureStatus()
    {
        return _printerState.CurrentErrorCodes.Contains(PrinterErrorCodes.OVERHEAT) ? "overheated" : "normal";
    }

    private string GetActiveErrorMessage()
    {
        var errorDetails = _printerState.CurrentErrorCodes
            .Select(GetErrorDetail);

        return $"Printer has active errors: {string.Join(" ", errorDetails)}";
    }
}
