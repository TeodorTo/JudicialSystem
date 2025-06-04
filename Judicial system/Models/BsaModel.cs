using System.ComponentModel.DataAnnotations;

namespace Judicial_system.Models;

public class BsaModel
{
    [Required]
    [Range(50, 250)]
    public double HeightCm { get; set; }  // Ръст в см

    [Required]
    [Range(20, 200)]
    public double WeightKg { get; set; }  // Тегло в кг

    public double? BsaResult { get; set; }
}
