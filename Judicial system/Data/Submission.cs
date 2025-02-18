using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity; 

namespace Judicial_system.Data;

public class Submission
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string SourceCode { get; set; } = null!; 

    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;

    [Range(0, 100)]
    public decimal Score { get; set; } 


    [Required]
    public string UserId { get; set; } = null!;

    [ForeignKey(nameof(UserId))]
    public IdentityUser User { get; set; } = null!;


    [Required]
    public int TaskId { get; set; }
    [ForeignKey(nameof(TaskId))]
    public Task Task { get; set; } = null!;
}
