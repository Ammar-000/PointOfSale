using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.FiltersModels;

public class ProductFilterModel
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }

}
