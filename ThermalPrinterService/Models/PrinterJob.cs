namespace ThermalPrinterService.Models;

public class PrinterJob
{
    public string JobId { get; init; } = "";

    public string Operation { get; init; } = "";

    public string Status { get; set; } = "";

    public DateTime UpdatedAt { get; set; }

    public string? Text { get; init; }

    public string? ImageFileName { get; init; }

    public string? ImageContentType { get; init; }

    public long? ImageSizeBytes { get; init; }
}
