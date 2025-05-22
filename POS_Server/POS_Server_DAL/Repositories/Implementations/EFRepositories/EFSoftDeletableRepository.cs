using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.EntityFrameworkCore;
using POS_Domains.Models;
using POS_Server_DAL.Repositories.Implementations.EFRepositories.Context;
using POS_Server_DAL.Repositories.Interfaces;
using POS_Server_DAL.Services;
using System.Linq.Expressions;

namespace POS_Server_DAL.Repositories.Implementations.EFRepositories;

public class EFSoftDeletableRepository<T> : EFBaseRepository<T>, ISoftDeletableRepository<T> where T : BaseSoftDeletableModel
{
    public EFSoftDeletableRepository(EFDbContext context, ICustomLogger logger,
        IExpressionCombiner<T> expressionCombiner, IIncludeNavPropsProvider<T>? includeNavPropsProvider = null)
        : base(context, logger, expressionCombiner, includeNavPropsProvider) { }

    public async Task<OperationResult<int>> CountAsync(bool includeInActive)
    {
        Expression<Func<T, bool>>? predicate = includeInActive ? null : e => e.IsActive;
        return await CountAsync(predicate);
    }

    public OperationResult<IQueryable<T>> GetAllQueryable(bool includeInActive)
    {
        Expression<Func<T, bool>>? predicate = includeInActive ? null : e => e.IsActive;
        return GetQueryableAsync(predicate, null, null);
    }

    public OperationResult<IQueryable<T>> FilterByQueryable(Expression<Func<T, bool>> predicate, bool includeInActive)
    {
        if (!includeInActive)
        {
            OperationResult<Expression<Func<T, bool>>> newPredicateRes = CombineAndTExpressions(predicate, e => e.IsActive);
            if (!newPredicateRes.Succeeded || newPredicateRes.Data == null)
            {
                newPredicateRes.AddError($"Failed to filter entities of '{EntityName}'");
                return OperationResult<IQueryable<T>>.Fail(newPredicateRes.Errors);
            }
            predicate = newPredicateRes.Data;
        }

        return GetQueryableAsync(predicate, null, null);
    }

    public async Task<OperationResult<List<T>>> GetAllAsync(bool includeInActive)
    {
        Expression<Func<T, bool>>? predicate = includeInActive ? null : e => e.IsActive;
        return await GetListAsync(predicate, null, null);
    }

    public async Task<OperationResult<List<T>>> FilterByAsync(Expression<Func<T, bool>> predicate, bool includeInActive)
    {
        if (!includeInActive)
        {
            OperationResult<Expression<Func<T, bool>>> newPredicateRes = CombineAndTExpressions(predicate, e => e.IsActive);
            if (!newPredicateRes.Succeeded || newPredicateRes.Data == null)
            {
                newPredicateRes.AddError($"Failed to filter entities of '{EntityName}'");
                return OperationResult<List<T>>.Fail(newPredicateRes.Errors);
            }
            predicate = newPredicateRes.Data;
        }

        return await GetListAsync(predicate, null, null);
    }

    public async Task<OperationResult<List<T>>> GetAllPagedAsync(int pageQty, int pageNum, bool includeInActive)
    {
        Expression<Func<T, bool>>? predicate = includeInActive ? null : e => e.IsActive;
        return await GetListAsync(predicate, pageQty, pageNum);
    }

    public async Task<OperationResult<List<T>>> FilterByPagedAsync(Expression<Func<T, bool>> predicate, int pageQty, int pageNum, bool includeInActive)
    {
        if (!includeInActive)
        {
            OperationResult<Expression<Func<T, bool>>> newPredicateRes = CombineAndTExpressions(predicate, e => e.IsActive);
            if (!newPredicateRes.Succeeded || newPredicateRes.Data == null)
            {
                newPredicateRes.AddError($"Failed to filter entities of '{EntityName}'");
                return OperationResult<List<T>>.Fail(newPredicateRes.Errors);
            }
            predicate = newPredicateRes.Data;
        }

        return await GetListAsync(predicate, pageQty, pageNum);
    }

    public async Task<OperationResult<int>> SoftDeleteAsync(int entityId)
    {
        if (entityId <= 0) return OperationResult<int>.Fail("Entity Id cannot less than or equal 0.");

        OperationResult<T> fetchingResult = await GetByIdAsync(entityId);
        if (!fetchingResult.Succeeded || fetchingResult.Data == null) return OperationResult<int>.Fail($"Failed to get entity {EntityName} by Id");
        T entity = fetchingResult.Data;
        entity.IsActive = false;
        _dbSet.Update(entity);

        try
        {
            int numChanges = await _context.SaveChangesAsync();
            _context.Entry(entity).State = EntityState.Detached;
            return numChanges > 0
                ? OperationResult<int>.Success(entityId)
                : OperationResult<int>.Fail($"Failed to soft-delete {EntityName}.");
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on soft deleting '{EntityName}'.");
            return OperationResult<int>.Fail($"An error occurred while soft deleting '{EntityName}'.");
        }
    }

    public async Task<OperationResult<T>> RestoreAsync(int entityId)
    {
        if (entityId <= 0) return OperationResult<T>.Fail("Entity Id can't be less than or equal 0.");

        OperationResult<T> fetchingResult = await GetByIdAsync(entityId);
        if (!fetchingResult.Succeeded || fetchingResult.Data == null)
        {
            fetchingResult.AddError($"Can't fetch soft-deleted '{EntityName}'");
            return fetchingResult;
        }

        T entity = fetchingResult.Data;
        entity.IsActive = true;
        _dbSet.Update(entity);

        try
        {
            int numChanges = await _context.SaveChangesAsync();
            _context.Entry(entity).State = EntityState.Detached;
            if (!(numChanges > 0)) return OperationResult<T>.Fail($"Failed to restore {EntityName}.");
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on restoring '{EntityName}'.");
            return OperationResult<T>.Fail($"An error occurred while restoring '{EntityName}'.");
        }

        OperationResult<T> addedEtityRes = await GetByIdAsync(entity.Id!.Value);
        return addedEtityRes.Succeeded && addedEtityRes.Data != null
            ? addedEtityRes
            : OperationResult<T>.Success(null, $"{EntityName} has been restored successfully, but failed to fetch it, " +
                $"Errors: \r\n'{string.Join("\r\n", addedEtityRes.Errors)}'");
    }
}
