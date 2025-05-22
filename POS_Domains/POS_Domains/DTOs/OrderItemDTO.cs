using System.ComponentModel.DataAnnotations;

namespace POS_Domains.DTOs;

public class OrderItemDTO : BaseDTO
{
    [Required]
    [Range(1, int.MaxValue)]
    public int? Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal? ProductPrice { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal? SubTotalPrice { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? OrderId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? ProductId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(450)]
    public string? UpdatedBy { get; set; }
}