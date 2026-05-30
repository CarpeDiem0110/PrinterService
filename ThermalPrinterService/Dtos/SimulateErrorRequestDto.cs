using System.ComponentModel.DataAnnotations;
using ThermalPrinterService.Models;

namespace ThermalPrinterService.Dtos;

public class SimulateErrorRequestDto : IValidatableObject
{
    public List<string> ErrorCode { get; set; } = new();

    public IEnumerable<string> GetRequestedErrorCodes()
    {
        foreach (var errorCode in ErrorCode)
        {
            yield return errorCode;
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var requestedErrorCodes = GetRequestedErrorCodes().ToList();

        if (requestedErrorCodes.Count == 0)
        {
            yield return new ValidationResult(
                "At least one error code is required.",
                new[] { nameof(ErrorCode) });

            yield break;
        }

        foreach (var errorCode in requestedErrorCodes)
        {
            if (!IsValidErrorCode(errorCode))
            {
                yield return new ValidationResult(
                    $"Invalid error code: {errorCode}",
                    new[] { nameof(ErrorCode) });
            }
        }
    }

    private static bool IsValidErrorCode(string errorCode)
    {
        return errorCode is
            PrinterErrorCodes.PAPER_OUT or
            PrinterErrorCodes.PAPER_JAM or
            PrinterErrorCodes.COVER_OPEN or
            PrinterErrorCodes.OVERHEAT or
            PrinterErrorCodes.COMM_ERROR or
            PrinterErrorCodes.UNKNOWN_COMMAND;
    }
}
