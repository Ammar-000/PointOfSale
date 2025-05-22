using System.ComponentModel.DataAnnotations;

namespace POS_Domains.DTOs;

public class RoleDTO
{
    [Key]
    [MaxLength(450)]
    public string Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public string? Description { get; set; }
}