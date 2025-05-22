using Helper.CustomDataAnnotationsAttribut;
using System.ComponentModel.DataAnnotations;

namespace POS_Domains.Models;

public class OrderModel : BaseModel
{
    [Required]
    [Range(1, 200)]
    public int? TableNumber { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal? TotalPrice { get; set; }

    [Required]
    [MinCollectionCount<OrderItemModel>(1)]
    public virtual List<OrderItemModel> OrderItems { get; set; } = new();
}