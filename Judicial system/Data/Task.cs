using System.ComponentModel.DataAnnotations;

namespace Judicial_system.Data;


public enum TaskType
{
    Method,  
    Class,   
    ConsoleIO 
}
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
    
    public TaskType Type { get; set; } 

    public DateTime CreatedAt { get; set; }
    
    public string? FilePath { get; set; }
}
