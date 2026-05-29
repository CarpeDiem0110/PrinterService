using System.ComponentModel.DataAnnotations;

namespace ThermalPrinterService.Dtos;

public class PrintTextRequestDto
{
    [Required(ErrorMessage = "Text is required.")]
    [MinLength(1, ErrorMessage = "Text cannot be empty.")]
    [MaxLength(1000, ErrorMessage = "Text is too long.")]
    public string Text { get; set; } = "";
}
