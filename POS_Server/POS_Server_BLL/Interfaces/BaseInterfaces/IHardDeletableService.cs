using Helper.DataContainers;
using POS_Domains.Models;
using System.Linq.Expressions;

namespace POS_Server_BLL.Interfaces.BaseInterfaces;

public interface IHardDeletableService<TModel> : IBaseService<TModel> where TModel : BaseModel
{
    public Task<OperationResult<int>> CountAsync();
    public Task<OperationResult<List<TModel>>> GetAllAsync();
    public Task<OperationResult<List<TModel>>> FilterByAsync(Expression<Func<TModel, bool>> predicate);
    public Task<OperationResult<List<TModel>>> GetAllPagedAsync(int pageQty, int pageNum);
    public Task<OperationResult<List<TModel>>> FilterByPagedAsync(Expression<Func<TModel, bool>> predicate, int pageQty, int pageNum);
    public Task<OperationResult<int>> HardDeleteAsync(int entityId, string currUserId);
}
