namespace POS_Server_PL.Models.RequestsModels.DTOsRequstsModels.FiltersModels;

public class OrderDTOFilterModel
{
    public int? TableNumber { get; set; }
    public decimal? TotalPrice { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
    public OrderDTOItemFilterModel? OrderItemFilter { get; set; }
}
