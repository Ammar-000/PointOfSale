using System.ComponentModel.DataAnnotations;

namespace POS_Domains.Models;

public class ProductModel : BaseSoftDeletableModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal? Price { get; set; }

    public string? Description { get; set; }

    [MaxLength(500)]
    public string? ImageSubPath { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int? CategoryId { get; set; }


    public virtual CategoryModel? Category { get; set; }
    public virtual ICollection<OrderItemModel>? OrderItems { get; set; }
}
