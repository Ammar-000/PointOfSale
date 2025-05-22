using System.ComponentModel.DataAnnotations;

namespace POS_Domains.Models;

public abstract class BaseModel
{
    [Key]
    [Required]
    [Range(1,int.MaxValue)]
    public int? Id { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(450)]
    public string? UpdatedBy { get; set; }
}
