using System.ComponentModel.DataAnnotations;

namespace ThermalPrinterService.Dtos;

public class PrintImageRequestDto
{
    [Required(ErrorMessage = "ImageBase64 is required.")]
    [MinLength(1, ErrorMessage = "ImageBase64 cannot be empty.")]
    public string ImageBase64 { get; set; } = "";
}
