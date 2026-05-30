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
        return new PrinterHealthStatusDto
        {
            Paper = GetPaperStatus(),
            Cover = GetCoverStatus(),
            Temperature = GetTemperatureStatus(),
            CurrentErrorCode = _printerState.CurrentErrorCode,
            CurrentErrorDetail = _printerState.CurrentErrorCode is null
                ? null
                : GetErrorDetail(_printerState.CurrentErrorCode)
        };
    }

    public void CheckPrinterState(string operation, string jobId)
    {
        if (!_printerState.IsConnected)
        {
            AddErrorLog(operation, jobId, PrinterErrorCodes.COMM_ERROR);

            throw new ApiException(
                StatusCodes.Status409Conflict,
                GetErrorDetail(PrinterErrorCodes.COMM_ERROR));
        }

        if (_printerState.CurrentErrorCode != null)
        {
            AddErrorLog(operation, jobId, _printerState.CurrentErrorCode);

            throw new ApiException(
                StatusCodes.Status409Conflict,
                GetErrorDetail(_printerState.CurrentErrorCode));
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

    private void AddErrorLog(string operation, string jobId, string errorCode)
    {
        _printerLogService.AddLog(
            operation,
            "error",
            jobId,
            new LogError
            {
                Code = errorCode,
                Detail = GetErrorDetail(errorCode)
            }
        );
    }

    private string GetPaperStatus()
    {
        return _printerState.CurrentErrorCode switch
        {
            PrinterErrorCodes.PAPER_OUT => "out",
            PrinterErrorCodes.PAPER_JAM => "jammed",
            _ => "ok"
        };
    }

    private string GetCoverStatus()
    {
        return _printerState.CurrentErrorCode == PrinterErrorCodes.COVER_OPEN ? "open" : "closed";
    }

    private string GetTemperatureStatus()
    {
        return _printerState.CurrentErrorCode == PrinterErrorCodes.OVERHEAT ? "overheated" : "normal";
    }
}
