using Helper.DataContainers;
using POS_Domains.Models;
using System.Linq.Expressions;

namespace POS_Server_BLL.Interfaces.BaseInterfaces;

public interface ISoftDeletableService<TModel> : IBaseService<TModel> where TModel : BaseSoftDeletableModel
{
    public Task<OperationResult<int>> CountAsync(bool includeInActive = false);
    public Task<OperationResult<List<TModel>>> GetAllAsync(bool includeInActive = false);
    public Task<OperationResult<List<TModel>>> FilterByAsync(Expression<Func<TModel, bool>> predicate, bool includeInActive = false);
    public Task<OperationResult<List<TModel>>> GetAllPagedAsync(int pageQty, int pageNum, bool includeInActive = false);
    public Task<OperationResult<List<TModel>>> FilterByPagedAsync(Expression<Func<TModel, bool>> predicate,
        int pageQty, int pageNum, bool includeInActive = false);
    public Task<OperationResult<int>> SoftDeleteAsync(int entityId, string currUserId);
    public Task<OperationResult<TModel>> RestoreAsync(int entityId, string currUserId);
}
