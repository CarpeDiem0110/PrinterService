# Thermal Printer Service

Local ASP.NET Core service for simulating a thermal printer API. It supports USB/LAN connection mode selection, text/image print requests, printer status, error simulation, logs, CSV export, and retrying failed print jobs.

## Installation

Requirements:

- .NET SDK 9.0
- Docker Desktop, optional
- Windows, macOS, or Linux

Restore the project:

```bash
dotnet restore ThermalPrinterService/ThermalPrinterService.csproj
```

This project does not require a `.env` file. Default development settings are loaded from `ThermalPrinterService/appsettings.Development.json`.

The default local port is defined by the profile in `ThermalPrinterService/Properties/launchSettings.json`. When running with Docker, the container listens on port `8080`.

## Running

Run locally:

```bash
dotnet run --project ThermalPrinterService/ThermalPrinterService.csproj
```

After the application starts, open the URL shown in the terminal. Example:

```txt
http://localhost:5133
```

Run with Docker:

```bash
docker build -t thermal-printer-service .
docker run --rm -p 8080:8080 thermal-printer-service
```

When running with Docker, open:

```txt
http://localhost:8080
```

## Testing

Swagger UI is enabled in the development environment:

```txt
/swagger
```

Example curl requests:

```bash
curl -X POST http://localhost:5133/connect \
  -H "Content-Type: application/json" \
  -d "{\"mode\":\"usb\"}"
```

```bash
curl http://localhost:5133/status
```

```bash
curl -X POST http://localhost:5133/print/text \
  -H "Content-Type: application/json" \
  -d "{\"text\":\"Test receipt\"}"
```

```bash
curl -X POST http://localhost:5133/simulate-error \
  -H "Content-Type: application/json" \
  -d "{\"errorCode\":[\"PAPER_OUT\"]}"
```

```bash
curl -X POST http://localhost:5133/clear-error
```

```bash
curl http://localhost:5133/logs
```

```bash
curl -L http://localhost:5133/logs/export -o printer-logs.csv
```

Image print example:

```bash
curl -X POST http://localhost:5133/print/image \
  -F "image=@receipt.png"
```

Retry a failed job:

```bash
curl -X POST http://localhost:5133/reprint
```

If you are using Docker, replace the port in the examples with `8080`:

```txt
http://localhost:8080/status
```

## API Summary

| Method | Endpoint | Description |
| --- | --- | --- |
| POST | `/connect` | Selects the USB or LAN connection mode. |
| GET | `/status` | Returns connection, paper, cover, temperature, last job, and queue status. |
| POST | `/print/text` | Creates a simple text print request. |
| POST | `/print/image` | Creates an image print request. |
| GET | `/logs` | Returns operation logs as JSON. |
| GET | `/logs/export` | Downloads operation logs as CSV. |
| POST | `/reprint` | Attempts to retry the latest failed job. |
| POST | `/simulate-error` | Simulates printer error states. |
| POST | `/clear-error` | Clears active printer error states. |

Supported error codes:

- `PAPER_OUT`
- `PAPER_JAM`
- `COVER_OPEN`
- `OVERHEAT`
- `COMM_ERROR`
- `UNKNOWN_COMMAND`

## Architecture Notes

Folder structure:

```txt
ThermalPrinterService/
  Controllers/       HTTP endpoint layer
  Dtos/              Request/response models
  Exceptions/        API exception model
  Models/            Printer state, job, and log models
  Services/          Business logic services
  wwwroot/           HTML, CSS, and JavaScript UI
  Program.cs         ASP.NET Core application startup
```

Main services:

- `PrinterService`: Main coordination layer between the controller and lower-level services.
- `PrinterJobService`: Creates text/image print jobs, tracks job status, and handles reprint flow.
- `PrinterHealthService`: Checks connection and active printer errors.
- `PrinterLogService`: Stores successful and failed operations in memory.
- `LogExportService`: Converts logs to CSV format.

Assumptions:

- This service does not connect to a real thermal printer; it simulates printer behavior locally.
- Application state and logs are currently stored in memory. They are reset when the application restarts.
- Uploaded image files are saved to the `ImageUpload` folder while the application is running.
- The UI is served as static files from `wwwroot`.

## Development Notes

Build the project:

```bash
dotnet build ThermalPrinterService/ThermalPrinterService.csproj
```

No formatter or automated test project is currently defined. For additional verification, use Swagger or the curl examples above to test the API endpoints.
