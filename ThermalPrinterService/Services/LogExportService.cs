namespace ThermalPrinterService.Services;

using System.Text;
using ThermalPrinterService.Models;

public class LogExportService
{
    public string ExportAsCsv(IEnumerable<LogEntry> logs)
    {
        var csv = new StringBuilder();

        csv.AppendLine("Timestamp,Operation,Status,ConnectionMode,JobId,ErrorCodes,ErrorDetails");

        foreach (var log in logs)
        {
            csv.AppendLine(string.Join(
                ",",
                Escape(log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss")),
                Escape(log.Operation),
                Escape(log.Status),
                Escape(log.ConnectionMode),
                Escape(log.JobId),
                Escape(string.Join("; ", log.Errors.Select(error => error.Code))),
                Escape(string.Join("; ", log.Errors.Select(error => error.Detail)))));
        }

        return csv.ToString();
    }

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "";
        }

        var escapedValue = value.Replace("\"", "\"\"");

        return $"\"{escapedValue}\"";
    }
}
