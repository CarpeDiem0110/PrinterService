using Microsoft.AspNetCore.Mvc;
using ThermalPrinterService.Models;
using ThermalPrinterService.Services;

namespace ThermalPrinterService.Controllers;

[ApiController]
[Route("")]
public class PrinterController : ControllerBase
{
    private readonly PrinterService _printerService;

    public PrinterController(PrinterService printerService)
    {
        _printerService = printerService;
    }

    [HttpPost("connect")]
    public IActionResult Connect([FromBody] ConnectRequest request)
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
    public IActionResult PrintText([FromBody] PrintTextRequest request)
    {
    _printerService.PrintText(request.Text);

    return Ok(new
    {
        message = "Text printed successfully",
        text = request.Text
    });
    }


    [HttpPost("print/image")]
    public IActionResult PrintImage([FromBody] PrintImageRequest request)
    {
    _printerService.PrintImage(request.ImageBase64);

    return Ok(new
    {
        message = "Image printed successfully"
    });
    }


    [HttpGet("logs")]
    public IActionResult GetLogs()
    {
    return Ok(_printerService.GetLogs());
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
    public IActionResult SimulateError([FromBody] SimulateErrorRequest request)
    {
        _printerService.SimulateError(request.ErrorCode);

        return Ok(new
        {
            message = $"Simulated error: {request.ErrorCode}"
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