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


    
}