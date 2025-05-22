using POS_Domains.Models;
using POS_Server_BLL.Interfaces.BaseInterfaces;

namespace POS_Server_BLL.Interfaces.OtherInterfaces;

public interface IOrderService : IHardDeletableService<OrderModel>
{
}
