namespace POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.FiltersModels;

public class OrderItemFilterModel
{
    public int? Id { get; set; }
    public int? Quantity { get; set; }
    public decimal? ProductPrice { get; set; }
    public decimal? SubTotalPrice { get; set; }
    public int? ProductId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? UpdatedBy { get; set; }
}
