namespace ThermalPrinterService.Dtos;

public class PrinterStatusResponseDto
{
    public string Service { get; set; } = "";

    public ConnectionStatusDto Connection { get; set; } = new();

    public PrinterHealthStatusDto Health { get; set; } = new();

    public LastJobStatusDto? LastJob { get; set; }

    public QueueSummaryDto Queue { get; set; } = new();
}

public class ConnectionStatusDto
{
    public bool Connected { get; set; }

    public string? Mode { get; set; }
}

public class PrinterHealthStatusDto
{
    public string Paper { get; set; } = "ok";

    public string Cover { get; set; } = "closed";

    public string Temperature { get; set; } = "normal";

    public List<PrinterErrorStatusDto> CurrentErrors { get; set; } = new();
}

public class PrinterErrorStatusDto
{
    public string Code { get; set; } = "";

    public string Detail { get; set; } = "";
}

public class LastJobStatusDto
{
    public string Operation { get; set; } = "";

    public string Status { get; set; } = "";

    public string? JobId { get; set; }

    public DateTime Timestamp { get; set; }
}

public class QueueSummaryDto
{
    public int Pending { get; set; }

    public int Active { get; set; }

    public int Completed { get; set; }

    public int Failed { get; set; }
}
