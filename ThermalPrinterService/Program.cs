using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ThermalPrinterService.Exceptions;
using ThermalPrinterService.Models;
using ThermalPrinterService.Services;
using System.Diagnostics;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.UnmappedMemberHandling =
            JsonUnmappedMemberHandling.Disallow;
    });
    
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Services.AddSingleton<PrinterState>();
builder.Services.AddSingleton<PrinterLogService>();
builder.Services.AddSingleton<PrinterHealthService>();
builder.Services.AddSingleton<PrinterJobService>();
builder.Services.AddSingleton<PrinterService>();
builder.Services.AddSingleton<LogExportService>();

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;

        context.Response.ContentType = "application/problem+json";

        if (exception is ApiException apiException)
        {
            context.Response.StatusCode = apiException.StatusCode;

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = apiException.StatusCode,
                Title = "Request failed",
                Detail = apiException.Message
            });

            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server error",
            Detail = "Unexpected server error."
        });
    });
});

app.Use(async (context, next) =>
{
    var startedAt = Stopwatch.GetTimestamp();

    try
    {
        await next();
    }
    catch (ApiException apiException)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = apiException.StatusCode;

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = apiException.StatusCode,
            Title = "Request failed",
            Detail = apiException.Message
        });
    }
    finally
    {
        var elapsedMilliseconds = Stopwatch
            .GetElapsedTime(startedAt)
            .TotalMilliseconds;

        WriteRequestLog(
            context.Request.Method,
            context.Request.Path,
            context.Response.StatusCode,
            elapsedMilliseconds);
    }
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapControllers();

app.Run();

static void WriteRequestLog(
    string method,
    PathString path,
    int statusCode,
    double elapsedMilliseconds)
{
    var previousColor = Console.ForegroundColor;

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.Write($"[{DateTime.Now:HH:mm:ss}] ");

    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.Write($"{method,-6}");

    Console.ForegroundColor = previousColor;
    Console.Write($" {path} -> ");

    Console.ForegroundColor = GetStatusCodeColor(statusCode);
    Console.Write(statusCode);

    Console.ForegroundColor = ConsoleColor.DarkGray;
    Console.WriteLine($" {elapsedMilliseconds:0.0} ms");

    Console.ForegroundColor = previousColor;
}

static ConsoleColor GetStatusCodeColor(int statusCode)
{
    return statusCode switch
    {
        >= 200 and < 400 => ConsoleColor.Green,
        >= 400 and < 500 => ConsoleColor.Yellow,
        >= 500 => ConsoleColor.Red,
        _ => ConsoleColor.Gray
    };
}

public partial class Program;
