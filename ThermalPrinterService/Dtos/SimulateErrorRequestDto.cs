using System.ComponentModel.DataAnnotations;

namespace ThermalPrinterService.Dtos;

public class SimulateErrorRequestDto
{
    [Required(ErrorMessage = "ErrorCode is required.")]
    [RegularExpression(
        "^(PAPER_OUT|PAPER_JAM|COVER_OPEN|OVERHEAT|COMM_ERROR|UNKNOWN_COMMAND)$",
        ErrorMessage = "Invalid error code."
    )]
    public string ErrorCode { get; set; } = "";
}
