using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Controllers.Admin.BaseControllers;
using POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.FiltersModels;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace POS_Server_PL.Controllers.Admin;

public class OrdersController : HardDeletableController<OrderModel, IOrderService>
{
    public OrdersController(IOrderService hardService, ICustomLogger logger) : base(hardService, logger) { }

    [HttpPost("FilterBy")]
    public async Task<ActionResult<OperationResult<List<OrderModel>>>> FilterByAsync(
        [Required][FromBody] OrderFilterModel orderFilter)
    {
        return await FilterByAsync(CreateFilterPredicate(orderFilter));
    }

    [HttpPost("FilterByPaged")]
    public async Task<ActionResult<OperationResult<List<OrderModel>>>> FilterByPagedAsync(
        [Required][FromBody] OrderFilterModel orderFilter,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum)
    {
        return await FilterByPagedAsync(CreateFilterPredicate(orderFilter), pageQty, pageNum);
    }

    Expression<Func<OrderModel, bool>> CreateFilterPredicate(OrderFilterModel orderFilter)
    {
        return o =>
            (orderFilter.TableNumber == null || o.TableNumber == orderFilter.TableNumber) &&
            (orderFilter.TotalPrice == null || o.TotalPrice == orderFilter.TotalPrice) &&
            (orderFilter.CreatedAt == null || o.CreatedAt == orderFilter.CreatedAt) &&
            (orderFilter.CreatedBy == null || o.CreatedBy == orderFilter.CreatedBy) &&
            (orderFilter.UpdatedAt == null || o.UpdatedAt == orderFilter.UpdatedAt) &&
            (orderFilter.UpdatedBy == null || o.UpdatedBy == orderFilter.UpdatedBy) &&
            (orderFilter.OrderItemFilter == null || (
            (orderFilter.OrderItemFilter.Id == null || o.OrderItems.Any(oi => oi.Id == orderFilter.OrderItemFilter.Id)) &&
            (orderFilter.OrderItemFilter.Quantity == null || o.OrderItems.Any(oi => oi.Quantity == orderFilter.OrderItemFilter.Quantity)) &&
            (orderFilter.OrderItemFilter.ProductPrice == null || o.OrderItems.Any(oi => oi.ProductPrice == orderFilter.OrderItemFilter.ProductPrice)) &&
            (orderFilter.OrderItemFilter.SubTotalPrice == null || o.OrderItems.Any(oi => oi.SubTotalPrice == orderFilter.OrderItemFilter.SubTotalPrice)) &&
            (orderFilter.OrderItemFilter.ProductId == null || o.OrderItems.Any(oi => oi.ProductId == orderFilter.OrderItemFilter.ProductId)) &&
            (orderFilter.OrderItemFilter.CreatedAt == null || o.OrderItems.Any(oi => oi.CreatedAt == orderFilter.OrderItemFilter.CreatedAt)) &&
            (orderFilter.OrderItemFilter.CreatedBy == null || o.OrderItems.Any(oi => oi.CreatedBy == orderFilter.OrderItemFilter.CreatedBy)) &&
            (orderFilter.OrderItemFilter.UpdatedAt == null || o.OrderItems.Any(oi => oi.UpdatedAt == orderFilter.OrderItemFilter.UpdatedAt)) &&
            (orderFilter.OrderItemFilter.UpdatedBy == null || o.OrderItems.Any(oi => oi.UpdatedBy == orderFilter.OrderItemFilter.UpdatedBy))));
    }

}
