using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ThermalPrinterService.Dtos;

public class PrintImageRequestDto : IValidatableObject
{
    [Required(ErrorMessage = "Image is required.")]
    public IFormFile Image { get; set; } = null!;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Image is null)
        {
            yield break;
        }

        if (Image.Length == 0)
        {
            yield return new ValidationResult(
                "Image cannot be empty.",
                new[] { nameof(Image) });
        }

        if (!Image.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult(
                "File must be an image.",
                new[] { nameof(Image) });
        }
    }
}
