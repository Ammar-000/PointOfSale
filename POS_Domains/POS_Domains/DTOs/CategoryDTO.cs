using System.ComponentModel.DataAnnotations;

namespace POS_Domains.DTOs;

public class CategoryDTO : BaseDTO
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public string? Description { get; set; }
}
