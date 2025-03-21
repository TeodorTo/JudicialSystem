
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Judicial_system.Data;

public class Topic
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }


    public List<Task> Tasks { get; set; } = new List<Task>();
}