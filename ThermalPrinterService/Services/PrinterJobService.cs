namespace ThermalPrinterService.Services;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ThermalPrinterService.Dtos;
using ThermalPrinterService.Exceptions;
using ThermalPrinterService.Models;

public class PrinterJobService
{
    private readonly IWebHostEnvironment _environment;
    private readonly PrinterHealthService _printerHealthService;
    private readonly PrinterLogService _printerLogService;
    private readonly List<PrinterJob> _jobs = new();

    public PrinterJobService(
        IWebHostEnvironment environment,
        PrinterHealthService printerHealthService,
        PrinterLogService printerLogService)
    {
        _environment = environment;
        _printerHealthService = printerHealthService;
        _printerLogService = printerLogService;
    }

    public void PrintText(string text)
    {
        var job = CreateJob("print_text", text: text);
        ProcessJob(job);
    }

    public void PrintImage(IFormFile image)
    {
        var job = CreateJob(
            "print_image",
            imageFileName: image.FileName,
            imageContentType: image.ContentType,
            imageSizeBytes: image.Length);

        ProcessImageJob(job, image);
    }

    public void Reprint()
    {
        var job = _jobs
            .OrderByDescending(item => item.UpdatedAt)
            .FirstOrDefault(item => item.Status == PrinterJobStatuses.Failed);

        if (job is null)
        {
            throw new ApiException(
                StatusCodes.Status404NotFound,
                "No failed job found to reprint.");
        }

        ProcessJob(job, "reprint");
    }

    public LastJobStatusDto? GetLastJob()
    {
        var lastJob = _jobs
            .OrderByDescending(job => job.UpdatedAt)
            .FirstOrDefault();

        return lastJob is null
            ? null
            : new LastJobStatusDto
            {
                Operation = lastJob.Operation,
                Status = lastJob.Status,
                JobId = lastJob.JobId,
                Timestamp = lastJob.UpdatedAt
            };
    }

    public QueueSummaryDto GetQueueSummary()
    {
        return new QueueSummaryDto
        {
            Pending = _jobs.Count(job => job.Status == PrinterJobStatuses.Pending),
            Active = 0,
            Completed = _jobs.Count(job => job.Status == PrinterJobStatuses.Completed),
            Failed = _jobs.Count(job => job.Status == PrinterJobStatuses.Failed)
        };
    }

    private PrinterJob CreateJob(
        string operation,
        string? text = null,
        string? imageFileName = null,
        string? imageContentType = null,
        long? imageSizeBytes = null)
    {
        var job = new PrinterJob
        {
            JobId = Guid.NewGuid().ToString(),
            Operation = operation,
            Text = text,
            ImageFileName = imageFileName,
            ImageContentType = imageContentType,
            ImageSizeBytes = imageSizeBytes,
            Status = PrinterJobStatuses.Pending,
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
            _printerHealthService.CheckPrinterState(operation, job.JobId);

            job.Status = PrinterJobStatuses.Completed;
            job.UpdatedAt = DateTime.Now;

            _printerLogService.AddLog(operation, "success", job.JobId);
        }
        catch (ApiException)
        {
            job.Status = PrinterJobStatuses.Failed;
            job.UpdatedAt = DateTime.Now;
            throw;
        }
    }

    private void ProcessImageJob(PrinterJob job, IFormFile image)
    {
        try
        {
            _printerHealthService.CheckPrinterState(job.Operation, job.JobId);

            SaveImage(job.JobId, image);
            job.Status = PrinterJobStatuses.Completed;
            job.UpdatedAt = DateTime.Now;

            _printerLogService.AddLog(job.Operation, "success", job.JobId);
        }
        catch (ApiException)
        {
            job.Status = PrinterJobStatuses.Failed;
            job.UpdatedAt = DateTime.Now;
            throw;
        }
    }

    private void SaveImage(string jobId, IFormFile image)
    {
        var uploadDirectory = Path.Combine(_environment.ContentRootPath, "ImageUpload");
        Directory.CreateDirectory(uploadDirectory);

        var originalFileName = Path.GetFileName(image.FileName);
        var savedFileName = $"{jobId}_{originalFileName}";
        var savedPath = Path.Combine(uploadDirectory, savedFileName);

        using var fileStream = File.Create(savedPath);
        image.CopyTo(fileStream);
    }
}
