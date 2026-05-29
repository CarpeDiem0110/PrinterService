using System.ComponentModel.DataAnnotations;

namespace ThermalPrinterService.Dtos;

public class ConnectRequestDto
{
    [Required(ErrorMessage = "Mode is required.")]
    [RegularExpression("^(usb|lan)$", ErrorMessage = "Mode must be usb or lan.")]
    public string Mode { get; set; } = "";
}
