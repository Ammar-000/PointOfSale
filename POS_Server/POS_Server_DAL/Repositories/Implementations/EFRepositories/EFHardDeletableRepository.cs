using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.EntityFrameworkCore;
using POS_Domains.Models;
using POS_Server_DAL.Repositories.Implementations.EFRepositories.Context;
using POS_Server_DAL.Repositories.Interfaces;
using POS_Server_DAL.Services;
using System.Linq.Expressions;

namespace POS_Server_DAL.Repositories.Implementations.EFRepositories;

public class EFHardDeletableRepository<T> : EFBaseRepository<T>, IHardDeletableRepository<T> where T : BaseModel
{
    public EFHardDeletableRepository(EFDbContext context, ICustomLogger logger,
        IExpressionCombiner<T> expressionCombiner, IIncludeNavPropsProvider<T>? includeNavPropsProvider = null)
        : base(context, logger, expressionCombiner, includeNavPropsProvider) { }

    public async Task<OperationResult<int>> CountAsync()
    {
        return await CountAsync(null);
    }

    public OperationResult<IQueryable<T>> GetAllQueryable()
    {
        return GetQueryableAsync(null, null, null);
    }

    public OperationResult<IQueryable<T>> FilterByQueryable(Expression<Func<T, bool>> predicate)
    {
        return GetQueryableAsync(predicate, null, null);
    }

    public async Task<OperationResult<List<T>>> GetAllAsync()
    {
        return await GetListAsync(null, null, null);
    }

    public async Task<OperationResult<List<T>>> FilterByAsync(Expression<Func<T, bool>> predicate)
    {
        return await GetListAsync(predicate, null, null);
    }

    public async Task<OperationResult<List<T>>> GetAllPagedAsync(int pageQty, int pageNum)
    {
        return await GetListAsync(null, pageQty, pageNum);
    }

    public async Task<OperationResult<List<T>>> FilterByPagedAsync(Expression<Func<T, bool>> predicate, int pageQty, int pageNum)
    {
        return await GetListAsync(predicate, pageQty, pageNum);
    }

    public async Task<OperationResult<int>> HardDeleteAsync(int entityId)
    {
        if (entityId <= 0) return OperationResult<int>.Fail("Entity Id cannot less than or equal 0.");

        OperationResult<T> fetchingResult = await GetByIdAsync(entityId);
        if (!fetchingResult.Succeeded || fetchingResult.Data == null) return OperationResult<int>.Fail($"Failed to get entity {EntityName} by Id");
        T entity = fetchingResult.Data;
        _dbSet.Remove(entity);

        try
        {
            int numChanges = await _context.SaveChangesAsync();
            _context.Entry(entity).State = EntityState.Detached;
            return numChanges > 0
                ? OperationResult<int>.Success(entityId)
                : OperationResult<int>.Fail($"Failed to delete {EntityName}.");
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on hard deleting '{EntityName}'.");
            return OperationResult<int>.Fail($"An error occurred while deleting '{EntityName}'.");
        }
    }

    public async Task<OperationResult<List<int>>> HardDeleteRangeAsync(List<int> entityIds)
    {
        if (entityIds.Count == 0) return OperationResult<List<int>>.Fail("Entity Ids can't be empty.");

        List<T> entities = new();
        OperationResult<T> fetchingResult;
        foreach (int entityId in entityIds)
        {
            if (entityIds.Any(i => i <= 0)) return OperationResult<List<int>>.Fail("Entity Id can't be less than or equal 0.");

            fetchingResult = await GetByIdAsync(entityId);
            if (!fetchingResult.Succeeded || fetchingResult.Data == null)
                return OperationResult<List<int>>.Fail($"Failed to fetch entity {EntityName} by Id '{entityId}'.");
            entities.Add(fetchingResult.Data);
        }

        _dbSet.RemoveRange(entities);
        try
        {
            return await _context.SaveChangesAsync() > 0
                ? OperationResult<List<int>>.Success(entityIds)
                : OperationResult<List<int>>.Fail($"Failed to delete {EntityName}.");
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on hard deleting range of '{EntityName}'.");
            return OperationResult<List<int>>.Fail($"An error occurred while deleting range of '{EntityName}'.");
        }
    }

}
