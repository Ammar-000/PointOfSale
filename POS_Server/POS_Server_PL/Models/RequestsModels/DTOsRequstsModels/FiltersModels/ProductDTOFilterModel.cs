using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Models.RequestsModels.DTOsRequstsModels.FiltersModels;

public class ProductDTOFilterModel
{
    public string? Name { get; set; }
    public decimal? Price { get; set; }
    public string? Description { get; set; }
    public int? CategoryId { get; set; }
}
