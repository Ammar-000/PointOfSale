using System.ComponentModel.DataAnnotations;

namespace POS_Domains.Models;

public class OrderItemModel : BaseModel
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


    public virtual OrderModel? Order { get; set; }

    public virtual ProductModel? Product { get; set; }
}