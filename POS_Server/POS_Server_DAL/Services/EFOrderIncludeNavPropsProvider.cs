using Microsoft.EntityFrameworkCore;
using POS_Domains.Models;

namespace POS_Server_DAL.Services;

public class EFOrderIncludeNavPropsProvider : IIncludeNavPropsProvider<OrderModel>
{
    public IQueryable<OrderModel> ApplyIncludesNavigationProps(IQueryable<OrderModel> query)
    {
        return query.Include(o => o.OrderItems);
    }
}
