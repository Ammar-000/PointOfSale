using System.ComponentModel.DataAnnotations;

namespace POS_Domains.DTOs;

public abstract class BaseDTO
{
    [Key]
    [Required]
    [Range(1, int.MaxValue)]
    public int? Id { get; set; }
}
