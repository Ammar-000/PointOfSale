using Helper.DataContainers;
using POS_Domains.Models;
using System.Linq.Expressions;

namespace POS_Server_DAL.Repositories.Interfaces;

public interface IHardDeletableRepository<T> : IBaseRepository<T> where T : BaseModel
{
    public Task<OperationResult<int>> CountAsync();
    public OperationResult<IQueryable<T>> GetAllQueryable();
    public OperationResult<IQueryable<T>> FilterByQueryable(Expression<Func<T, bool>> predicate);
    public Task<OperationResult<List<T>>> GetAllAsync();
    public Task<OperationResult<List<T>>> FilterByAsync(Expression<Func<T, bool>> predicate);
    public Task<OperationResult<List<T>>> GetAllPagedAsync(int pageQty, int pageNum);
    public Task<OperationResult<List<T>>> FilterByPagedAsync(Expression<Func<T, bool>> predicate, int pageQty, int pageNum);
    public Task<OperationResult<int>> HardDeleteAsync(int entityId);
    public Task<OperationResult<List<int>>> HardDeleteRangeAsync(List<int> entityIds);
}
