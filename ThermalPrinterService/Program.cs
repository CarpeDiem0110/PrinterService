using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ThermalPrinterService.Exceptions;
using ThermalPrinterService.Models;
using ThermalPrinterService.Services;
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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

public partial class Program;
