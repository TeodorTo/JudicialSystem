using System.ComponentModel.DataAnnotations;

namespace Judicial_system.Data;

public class Task
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    public string Description { get; set; } = null!;

    [Required]
    public string UnitTestCode { get; set; } = null!; 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
