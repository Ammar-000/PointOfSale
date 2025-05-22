using Helper.CustomDataAnnotationsAttribut;
using System.ComponentModel.DataAnnotations;

namespace POS_Domains.DTOs;

public class OrderDTO : BaseDTO
{
    [Required]
    [Range(1, 200)]
    public int? TableNumber { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal? TotalPrice { get; set; }

    [Required]
    [MinCollectionCount<OrderItemDTO>(1)]
    public List<OrderItemDTO> OrderItems { get; set; } = new();

    [Required]
    public DateTime CreatedAt { get; set; }

    [Required]
    [MaxLength(450)]
    public string CreatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [MaxLength(450)]
    public string? UpdatedBy { get; set; }
}