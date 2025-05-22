using Helper.DataContainers;
using POS_Domains.Models;
using System.Linq.Expressions;

namespace POS_Server_DAL.Repositories.Interfaces;

public interface ISoftDeletableRepository<T> : IBaseRepository<T> where T : BaseSoftDeletableModel
{
    public Task<OperationResult<int>> CountAsync(bool includeInActive);
    public OperationResult<IQueryable<T>> GetAllQueryable(bool includeInActive);
    public OperationResult<IQueryable<T>> FilterByQueryable(Expression<Func<T, bool>> predicate, bool includeInActive);
    public Task<OperationResult<List<T>>> GetAllAsync(bool includeInActive);
    public Task<OperationResult<List<T>>> FilterByAsync(Expression<Func<T, bool>> predicate, bool includeInActive);
    public Task<OperationResult<List<T>>> GetAllPagedAsync(int pageQty, int pageNum, bool includeInActive);
    public Task<OperationResult<List<T>>> FilterByPagedAsync(Expression<Func<T, bool>> predicate,
        int pageQty, int pageNum, bool includeInActive);
    public Task<OperationResult<int>> SoftDeleteAsync(int entityId);
    public Task<OperationResult<T>> RestoreAsync(int entityId);
}
