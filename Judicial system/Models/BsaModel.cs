using System.ComponentModel.DataAnnotations;

namespace Judicial_system.Models;

public class BsaModel
{
    [Required]
    public double HeightCm { get; set; }  

    [Required]
    public double WeightKg { get; set; }  

    public double? BsaResult { get; set; }
    
    public double? BsaFullResult { get; set; }

}
