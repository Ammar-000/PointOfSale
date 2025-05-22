using System.ComponentModel.DataAnnotations;

namespace POS_Domains.Models;

public class CategoryModel : BaseSoftDeletableModel
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    public string? Description { get; set; }


    public virtual ICollection<ProductModel>? Products { get; set; }
}
