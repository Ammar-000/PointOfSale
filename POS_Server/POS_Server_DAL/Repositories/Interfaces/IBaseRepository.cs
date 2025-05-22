using Helper.DataContainers;
using POS_Domains.Models;

namespace POS_Server_DAL.Repositories.Interfaces;

public interface IBaseRepository<T> where T : BaseModel
{
    public Task<OperationResult<T>> GetByIdAsync(int id);
    public Task<OperationResult<T>> AddAsync(T entity);
    public Task<OperationResult<T>> UpdateAsync(T entity);
}