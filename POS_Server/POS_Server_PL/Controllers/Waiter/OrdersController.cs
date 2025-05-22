using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.DTOs;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Controllers.Waiter.BaseControllers;
using POS_Server_PL.Models.RequestsModels.DTOsRequstsModels.FiltersModels;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace POS_Server_PL.Controllers.Waiter;

public class OrdersController : HardDeletableController<OrderModel, OrderDTO, IOrderService>
{
    public OrdersController(IOrderService hardService, ICustomLogger logger) : base(hardService, logger) { }

    [HttpPost("FilterBy")]
    public async Task<ActionResult<OperationResult<List<OrderDTO>>>> FilterByAsync(
        [Required][FromBody] OrderDTOFilterModel orderDTOFilter)
    {
        return await FilterByAsync(CreateFilterPredicate(orderDTOFilter));
    }

    [HttpPost("FilterByPaged")]
    public async Task<ActionResult<OperationResult<List<OrderDTO>>>> FilterByPagedAsync(
        [Required][FromBody] OrderDTOFilterModel orderDTOFilter,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum)
    {
        return await FilterByPagedAsync(CreateFilterPredicate(orderDTOFilter), pageQty, pageNum);
    }

    Expression<Func<OrderModel, bool>> CreateFilterPredicate(OrderDTOFilterModel orderDTOFilter)
    {
        return o =>
            (orderDTOFilter.TableNumber == null || o.TableNumber == orderDTOFilter.TableNumber) &&
            (orderDTOFilter.TotalPrice == null || o.TotalPrice == orderDTOFilter.TotalPrice) &&
            (orderDTOFilter.CreatedAt == null || o.CreatedAt == orderDTOFilter.CreatedAt) &&
            (orderDTOFilter.CreatedBy == null || o.CreatedBy == orderDTOFilter.CreatedBy) &&
            (orderDTOFilter.UpdatedAt == null || o.UpdatedAt == orderDTOFilter.UpdatedAt) &&
            (orderDTOFilter.UpdatedBy == null || o.UpdatedBy == orderDTOFilter.UpdatedBy) &&
            (orderDTOFilter.OrderItemFilter == null || (
            (orderDTOFilter.OrderItemFilter.Id == null || o.OrderItems.Any(oi => oi.Id == orderDTOFilter.OrderItemFilter.Id)) &&
            (orderDTOFilter.OrderItemFilter.Quantity == null || o.OrderItems.Any(oi => oi.Quantity == orderDTOFilter.OrderItemFilter.Quantity)) &&
            (orderDTOFilter.OrderItemFilter.ProductPrice == null || o.OrderItems.Any(oi => oi.ProductPrice == orderDTOFilter.OrderItemFilter.ProductPrice)) &&
            (orderDTOFilter.OrderItemFilter.SubTotalPrice == null || o.OrderItems.Any(oi => oi.SubTotalPrice == orderDTOFilter.OrderItemFilter.SubTotalPrice)) &&
            (orderDTOFilter.OrderItemFilter.ProductId == null || o.OrderItems.Any(oi => oi.ProductId == orderDTOFilter.OrderItemFilter.ProductId)) &&
            (orderDTOFilter.OrderItemFilter.CreatedAt == null || o.OrderItems.Any(oi => oi.CreatedAt == orderDTOFilter.OrderItemFilter.CreatedAt)) &&
            (orderDTOFilter.OrderItemFilter.CreatedBy == null || o.OrderItems.Any(oi => oi.CreatedBy == orderDTOFilter.OrderItemFilter.CreatedBy)) &&
            (orderDTOFilter.OrderItemFilter.UpdatedAt == null || o.OrderItems.Any(oi => oi.UpdatedAt == orderDTOFilter.OrderItemFilter.UpdatedAt)) &&
            (orderDTOFilter.OrderItemFilter.UpdatedBy == null || o.OrderItems.Any(oi => oi.UpdatedBy == orderDTOFilter.OrderItemFilter.UpdatedBy))));
    }

}
