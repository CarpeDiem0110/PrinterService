namespace ThermalPrinterService.Models;

public class PrinterJob
{
    public string JobId { get; init; } = "";

    public string Operation { get; init; } = "";

    public string Status { get; set; } = "";

    public DateTime UpdatedAt { get; set; }

    public string? Text { get; init; }

    public string? ImageBase64 { get; init; }
}
