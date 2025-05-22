using System.ComponentModel.DataAnnotations;

namespace POS_Domains.Models;

public class RoleModel
{
    [Key]
    [MaxLength(450)]
    public string Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public string? Description { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(450)]
    public string? UpdatedBy { get; set; }
}