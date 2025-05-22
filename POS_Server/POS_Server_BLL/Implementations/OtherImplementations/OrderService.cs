using AutoMapper;
using Helper.DataContainers;
using Helper.Interfaces;
using POS_Domains.Models;
using POS_Server_BLL.Implementations.BaseImplementations;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_DAL.Repositories.Interfaces;

namespace POS_Server_BLL.Implementations.OtherImplementations;

public class OrderService : HardDeletableService<OrderModel>, IOrderService
{
    readonly IHardDeletableRepository<OrderItemModel> _orderItemRepository;
    readonly IUnitOfWork _unitOfWork;

    public OrderService(IHardDeletableRepository<OrderModel> OrderRepository,
        IHardDeletableRepository<OrderItemModel> orderItemRepository,
        IUnitOfWork unitOfWork, IEntityValidator<OrderModel> validator,
        IMapper mapper, ICustomLogger logger, IUserService userService)
        : base(OrderRepository, validator, mapper, logger, userService)
    {
        _orderItemRepository = orderItemRepository;
        _unitOfWork = unitOfWork;
    }

    #region Order main methods

    public override async Task<OperationResult<OrderModel>> AddAsync(OrderModel order, string currUserId)
    {
        OperationResult chkRes = await PrepareToAddUpd(order, true);
        if (!chkRes.Succeeded)
        {
            chkRes.AddError("Failed to add order.");
            return OperationResult<OrderModel>.Fail(chkRes.Errors);
        }
        return await base.AddAsync(order, currUserId);
    }

    public override async Task<OperationResult<OrderModel>> UpdateAsync(OrderModel order, string currUserId)
    {
        OperationResult result = await PrepareToAddUpd(order, false);
        if (!result.Succeeded)
        {
            result.AddError("Failed to update order.");
            return OperationResult<OrderModel>.Fail(result.Errors);
        }

        await _unitOfWork.BeginTransactionAsync();
        result = await DeleteRemovedOrderItems(order);
        if (!result.Succeeded)
        {
            result.AddError("Failed to update order.");
            await _unitOfWork.RollbackAsync();
            return OperationResult<OrderModel>.Fail(result.Errors);
        }
        order.TotalPrice = order.OrderItems.Sum(oi => oi.SubTotalPrice);

        OperationResult<OrderModel> updateRes = await base.UpdateAsync(order, currUserId);
        if (updateRes.Succeeded) await _unitOfWork.CommitAsync();
        else await _unitOfWork.RollbackAsync();

        return updateRes;
    }

    #endregion

    #region Order items main methods

    async Task<OperationResult> ResetNewOrderItemsIds(OrderModel updatedOrder)
    {
        OperationResult<OrderModel> origOrderRes = await GetByIdAsync(updatedOrder.Id!.Value);
        if (!origOrderRes.Succeeded || origOrderRes.Data == null || origOrderRes.Data.OrderItems == null)
        {
            origOrderRes.AddError("Failed to fetch original order.");
            return OperationResult.Fail(origOrderRes.Errors);
        }

        foreach (OrderItemModel updOrderItem in updatedOrder.OrderItems)
            if (!origOrderRes.Data.OrderItems.Any(ooi => ooi.Id == updOrderItem.Id)) updOrderItem.Id = default;

        return OperationResult.Success();
    }

    async Task<OperationResult> DeleteRemovedOrderItems(OrderModel updatedOrder)
    {
        OperationResult<OrderModel> origOrderRes = await GetByIdAsync(updatedOrder.Id!.Value);
        if (!origOrderRes.Succeeded || origOrderRes.Data == null || origOrderRes.Data.OrderItems == null)
        {
            origOrderRes.AddError("Failed to fetch original order.");
            return OperationResult.Fail(origOrderRes.Errors);
        }

        List<int> removedOrderItemsIds = new();
        foreach (OrderItemModel origOrderItem in origOrderRes.Data.OrderItems)
            if (!updatedOrder.OrderItems.Any(uoi => uoi.Id == origOrderItem.Id))
                removedOrderItemsIds.Add(origOrderItem.Id!.Value);

        if (removedOrderItemsIds.Count == 0) return OperationResult.Success();
        return await DeleteOrderItems(removedOrderItemsIds);
    }

    async Task<OperationResult> DeleteOrderItems(List<int> ids)
    {
        OperationResult<List<int>> delOrderItemsRes = await _orderItemRepository.HardDeleteRangeAsync(ids);
        return delOrderItemsRes.Succeeded ? OperationResult.Success() : OperationResult.Fail(delOrderItemsRes.Errors);
    }

    #endregion

    #region Helper methods

    async Task<OperationResult> PrepareToAddUpd(OrderModel order, bool isNew)
    {
        OperationResult result = PreAddUpdChks(order);
        if (!result.Succeeded) return result;
        PreparePrices(order);

        if (isNew) foreach (OrderItemModel orderItem in order.OrderItems) orderItem.Id = default;
        else result = await ResetNewOrderItemsIds(order);
        if (!result.Succeeded) return result;

        return result;
    }

    OperationResult PreAddUpdChks(OrderModel order)
    {
        if (order == null) return OperationResult.Fail("Order can't be null.");
        if (order.OrderItems == null || order.OrderItems.Count == 0)
            return OperationResult.Fail("Order must have order items.");
        if (order.Id == null) return OperationResult.Fail("Order id can't be null.");

        OperationResult chkRes = ChkDuplicateProducts(order);
        if (!chkRes.Succeeded) return chkRes;

        return chkRes;
    }

    OperationResult ChkDuplicateProducts(OrderModel order)
    {
        return order.OrderItems.Count == order.OrderItems.Select(oi => oi.ProductId).ToHashSet().Count
            ? OperationResult.Success()
            : OperationResult.Fail("Order can't have more than one order item with each product.");
    }

    void PreparePrices(OrderModel order)
    {
        order.TotalPrice = 0;
        foreach (OrderItemModel orderItem in order.OrderItems)
        {
            orderItem.SubTotalPrice = orderItem.ProductPrice * orderItem.Quantity;
            order.TotalPrice += orderItem.SubTotalPrice;
        }
    }

    #endregion

    #region Other methods

    public override void RemoveNavigationProps(OrderModel order)
    {
        foreach (OrderItemModel orderItem in order.OrderItems)
        {
            orderItem.Product = null;
            orderItem.Order = null;
        }
    }

    protected override async Task<OperationResult> SetCreatedProps(OrderModel order, string currUserId, bool isOrderNew)
    {
        OperationResult result = await base.SetCreatedProps(order, currUserId, isOrderNew);
        if (!result.Succeeded) return result;

        foreach (OrderItemModel orderItem in order.OrderItems)
        {
            // Order is new
            if (isOrderNew)
            {
                orderItem.CreatedAt = order.CreatedAt;
                orderItem.CreatedBy = order.CreatedBy;
                continue;
            }

            // Order is updated but order item is new
            if (orderItem.Id == null)
            {
                orderItem.CreatedAt = (DateTime)order.UpdatedAt!;
                orderItem.CreatedBy = order.UpdatedBy!;
                continue;
            }

            // Order is updated and order item is updated
            OperationResult<OrderItemModel> origOriItemRes = await _orderItemRepository.GetByIdAsync((int)orderItem.Id);
            if (!origOriItemRes.Succeeded || origOriItemRes.Data == null)
            {
                origOriItemRes.AddError($"Failed to fetch original order item with Id '{(int)orderItem.Id}'");
                return OperationResult.Fail(origOriItemRes.Errors);
            }
            orderItem.CreatedAt = origOriItemRes.Data.CreatedAt;
            orderItem.CreatedBy = origOriItemRes.Data.CreatedBy;
        }

        return result;
    }

    protected override void SetUpdatedProps(OrderModel order, string currUserId, bool isOrderNew)
    {
        base.SetUpdatedProps(order, currUserId, isOrderNew);

        foreach (OrderItemModel orderItem in order.OrderItems)
        {
            // Order item is new
            if (orderItem.Id == null)
            {
                orderItem.UpdatedAt = null;
                orderItem.UpdatedBy = null!;
                continue;
            }

            // Order item is updated and order must be also updated
            orderItem.UpdatedAt = order.UpdatedAt;
            orderItem.UpdatedBy = order.UpdatedBy;
        }
    }

    #endregion

}
