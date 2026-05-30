namespace ThermalPrinterService.Services;

using Microsoft.AspNetCore.Http;
using ThermalPrinterService.Dtos;
using ThermalPrinterService.Exceptions;
using ThermalPrinterService.Models;

public class PrinterService
{
    private const string JobStatusPending = "pending";
    private const string JobStatusCompleted = "completed";
    private const string JobStatusFailed = "failed";

    public string? ConnectionMode { get; private set; }
    public bool IsConnected { get; private set; }

    private string? _currentErrorCode;
    private readonly List<LogEntry> _logs = new();
    private readonly List<PrinterJob> _jobs = new();

    public void Connect(string mode)
    {
        ConnectionMode = mode;
        IsConnected = true;

        AddLog("connect", "success");
    }

    public PrinterStatusResponseDto GetStatus()
    {
        var lastJob = _jobs
            .OrderByDescending(job => job.UpdatedAt)
            .FirstOrDefault();

        return new PrinterStatusResponseDto
        {
            Service = "Thermal Printer Service",
            Connection = new ConnectionStatusDto
            {
                Connected = IsConnected,
                Mode = ConnectionMode
            },
            Health = new PrinterHealthStatusDto
            {
                Paper = GetPaperStatus(),
                Cover = GetCoverStatus(),
                Temperature = GetTemperatureStatus(),
                CurrentErrorCode = _currentErrorCode,
                CurrentErrorDetail = _currentErrorCode is null
                    ? null
                    : GetErrorDetail(_currentErrorCode)
            },
            LastJob = lastJob is null
                ? null
                : new LastJobStatusDto
                {
                    Operation = lastJob.Operation,
                    Status = lastJob.Status,
                    JobId = lastJob.JobId,
                    Timestamp = lastJob.UpdatedAt
                },
            Queue = new QueueSummaryDto
            {
                Pending = 0,
                Active = 0,
                Completed = _jobs.Count(job => job.Status == JobStatusCompleted),
                Failed = _jobs.Count(job => job.Status == JobStatusFailed)
            }
        };
    }

    public void PrintText(string text)
    {
        var job = CreateJob("print_text", text: text);
        ProcessJob(job);
    }

    public void PrintImage(string imageBase64)
    {
        var job = CreateJob("print_image", imageBase64: imageBase64);
        ProcessJob(job);
    }

    public List<LogEntry> GetLogs()
    {
        return _logs;
    }


    public void Reprint()
    {
        var job = _jobs
            .OrderByDescending(item => item.UpdatedAt)
            .FirstOrDefault(item => item.Status == JobStatusFailed);

        if (job is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "No failed job found to reprint.");
        }

        ProcessJob(job, "reprint");
    }

    public void SimulateError(string errorCode)
    {
        _currentErrorCode = errorCode;
        AddLog("simulate_error", "success");
    }

    public void ClearError()
    {
        _currentErrorCode = null;
        AddLog("clear_error", "success");
    }

    private void AddLog(
        string operation,
        string status,
        string? jobId = null,
        LogError? error = null)
    {
        _logs.Add(new LogEntry
        {
            Timestamp = DateTime.Now,
            Operation = operation,
            Status = status,
            ConnectionMode = ConnectionMode,
            JobId = jobId,
            Error = error
        });
    }

    private string GetErrorDetail(string errorCode)
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

    private PrinterJob CreateJob(
        string operation,
        string? text = null,
        string? imageBase64 = null)
    {
        var job = new PrinterJob
        {
            JobId = Guid.NewGuid().ToString(),
            Operation = operation,
            Text = text,
            ImageBase64 = imageBase64,
            Status = JobStatusPending,
            UpdatedAt = DateTime.Now
        };

        _jobs.Add(job);
        return job;
    }

    private void ProcessJob(PrinterJob job, string? logOperationOverride = null)
    {
        var operation = logOperationOverride ?? job.Operation;

        try
        {
            CheckPrinterState(operation, job.JobId);

            job.Status = JobStatusCompleted;
            job.UpdatedAt = DateTime.Now;

            AddLog(operation, "success", job.JobId);
        }
        catch (ApiException)
        {
            job.Status = JobStatusFailed;
            job.UpdatedAt = DateTime.Now;
            throw;
        }
    }

    private string GetPaperStatus()
    {
        return _currentErrorCode switch
        {
            PrinterErrorCodes.PAPER_OUT => "out",
            PrinterErrorCodes.PAPER_JAM => "jammed",
            _ => "ok"
        };
    }

    private string GetCoverStatus()
    {
        return _currentErrorCode == PrinterErrorCodes.COVER_OPEN ? "open" : "closed";
    }

    private string GetTemperatureStatus()
    {
        return _currentErrorCode == PrinterErrorCodes.OVERHEAT ? "overheated" : "normal";
    }

    private void CheckPrinterState(string operation, string jobId)
    {
        if (!IsConnected)
        {
            AddLog(
                operation,
                "error",
                jobId,
                new LogError
                {
                    Code = PrinterErrorCodes.COMM_ERROR,
                    Detail = GetErrorDetail(PrinterErrorCodes.COMM_ERROR)
                }
            );

            throw new ApiException(
                StatusCodes.Status409Conflict,
                GetErrorDetail(PrinterErrorCodes.COMM_ERROR));
        }

        if (_currentErrorCode != null)
        {
            AddLog(
                operation,
                "error",
                jobId,
                new LogError
                {
                    Code = _currentErrorCode,
                    Detail = GetErrorDetail(_currentErrorCode)
                }
            );

            throw new ApiException(
                StatusCodes.Status409Conflict,
                GetErrorDetail(_currentErrorCode));
        }
    }

    private sealed class PrinterJob
    {
        public string JobId { get; init; } = "";

        public string Operation { get; init; } = "";

        public string Status { get; set; } = JobStatusPending;

        public DateTime UpdatedAt { get; set; }

        public string? Text { get; init; }

        public string? ImageBase64 { get; init; }
    }
}
