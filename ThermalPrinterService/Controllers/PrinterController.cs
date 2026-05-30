using Microsoft.AspNetCore.Mvc;
using ThermalPrinterService.Dtos;
using ThermalPrinterService.Services;

namespace ThermalPrinterService.Controllers;

[ApiController]
[Route("")]
public class PrinterController : ControllerBase
{
    private readonly PrinterService _printerService;
    private readonly LogExportService _logExportService;

    public PrinterController(
        PrinterService printerService,
        LogExportService logExportService)
    {
        _printerService = printerService;
        _logExportService = logExportService;
    }

    [HttpPost("connect")]
    public IActionResult Connect([FromBody] ConnectRequestDto request)
    {
        _printerService.Connect(request.Mode);

        return Ok(new
        {
            message = $"Connected via {request.Mode}"
        });
    }

    [HttpGet("status")]
    public IActionResult Status()
    {
        return Ok(_printerService.GetStatus());
    }

    [HttpPost("print/text")]
    public IActionResult PrintText([FromBody] PrintTextRequestDto request)
    {
        _printerService.PrintText(request.Text);

        return Ok(new
        {
            message = "Text printed successfully",
            text = request.Text
        });
    }

    [HttpPost("print/image")]
    public IActionResult PrintImage([FromForm] PrintImageRequestDto request)
    {
        _printerService.PrintImage(request.Image);

        return Ok(new
        {
            message = "Image print request processed.",
            fileName = request.Image.FileName,
            contentType = request.Image.ContentType,
            size = request.Image.Length
        });
    }


    [HttpGet("logs")]
    public IActionResult GetLogs()
    {
        return Ok(_printerService.GetLogs());
    }

    [HttpGet("logs/export")]
    public IActionResult ExportLogs()
    {
        var csv = _logExportService.ExportAsCsv(_printerService.GetLogs());
        var fileName = $"printer-logs-{DateTime.Now:yyyyMMddHHmmss}.csv";

        return File(
            System.Text.Encoding.UTF8.GetBytes(csv),
            "text/csv",
            fileName);
    }

    [HttpPost("reprint")]
    public IActionResult Reprint()  
    {
        _printerService.Reprint();

        return Ok(new
        {
            message = "Last print reprinted successfully"
        });
    }


    [HttpPost("simulate-error")]
    public IActionResult SimulateError([FromBody] SimulateErrorRequestDto request)
    {
        var errorCodes = request.GetRequestedErrorCodes().Distinct().ToList();

        _printerService.SimulateError(errorCodes);

        return Ok(new
        {
            message = "Simulated errors updated.",
            errorCodes
        });
    }


    [HttpPost("clear-error")]
    public IActionResult ClearError()
    {
        _printerService.ClearError();

        return Ok(new
        {
            message = "Printer error cleared."
        });
    }

   


    
}
