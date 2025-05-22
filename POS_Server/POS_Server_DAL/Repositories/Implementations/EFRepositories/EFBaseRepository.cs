using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.EntityFrameworkCore;
using POS_Domains.Models;
using POS_Server_DAL.Repositories.Implementations.EFRepositories.Context;
using POS_Server_DAL.Repositories.Interfaces;
using POS_Server_DAL.Services;
using System.Linq.Expressions;

namespace POS_Server_DAL.Repositories.Implementations.EFRepositories;

public class EFBaseRepository<T> : IBaseRepository<T> where T : BaseModel
{
    protected readonly EFDbContext _context;
    protected readonly ICustomLogger _logger;
    protected readonly DbSet<T> _dbSet;
    protected readonly IExpressionCombiner<T> _expressionCombiner;
    protected readonly IIncludeNavPropsProvider<T>? _includeNavPropsProvider;

    protected string EntityName => typeof(T).Name;
    protected string LayerName => "DAL";

    public EFBaseRepository(EFDbContext context, ICustomLogger logger, IExpressionCombiner<T> expressionCombiner,
        IIncludeNavPropsProvider<T>? includeNavPropsProvider = null)
    {
        _context = context;
        _logger = logger;
        _dbSet = _context.Set<T>();
        _expressionCombiner = expressionCombiner;
        _includeNavPropsProvider = includeNavPropsProvider;
    }

    public async Task<OperationResult<int>> CountAsync(Expression<Func<T, bool>>? predicate)
    {
        int count;
        try
        {
            count = predicate == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(predicate);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on retrieving count of '{EntityName}'.");
            return OperationResult<int>.Fail($"An error occurred while retrieving count of '{EntityName}'.");
        }

        return OperationResult<int>.Success(count);
    }

    public async Task<OperationResult<T>> GetByIdAsync(int id)
    {
        T? entity;
        IQueryable<T> baseQuery = _dbSet;
        if (_includeNavPropsProvider != null) baseQuery = _includeNavPropsProvider.ApplyIncludesNavigationProps(baseQuery);
        try
        {
            entity = await baseQuery.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on getting {EntityName} by Id.");
            return OperationResult<T>.Fail($"An error occurred while retrieving the '{EntityName}' by Id.");
        }

        return entity == null
            ? OperationResult<T>.Fail($"Entity with Id '{id}' not found.")
            : OperationResult<T>.Success(entity);
    }

    public async Task<OperationResult<T>> AddAsync(T entity)
    {
        if (entity == null) return OperationResult<T>.Fail($"'{EntityName}' can't be null");
        _dbSet.Add(entity);

        try
        {
            int numChanges = await _context.SaveChangesAsync();
            _context.Entry(entity).State = EntityState.Detached;
            if (!(numChanges > 0)) return OperationResult<T>.Fail($"Failed to add {EntityName}.");
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on adding new '{EntityName}'.");
            return OperationResult<T>.Fail($"An error occurred while adding the new '{EntityName}'.");
        }

        OperationResult<T> addedEtityRes = await GetByIdAsync(entity.Id!.Value);
        return addedEtityRes.Succeeded && addedEtityRes.Data != null
            ? addedEtityRes
            : OperationResult<T>.Success(null, $"{EntityName} has been added successfully, but failed to fetch it, " +
                $"Errors: \r\n'{string.Join("\r\n", addedEtityRes.Errors)}'");
    }

    public async Task<OperationResult<T>> UpdateAsync(T entity)
    {
        if (entity == null) return OperationResult<T>.Fail("Entity can't be null");
        _dbSet.Update(entity);

        try
        {
            int numChanges = await _context.SaveChangesAsync();
            _context.Entry(entity).State = EntityState.Detached;
            if (!(numChanges > 0)) return OperationResult<T>.Fail($"Failed to update {EntityName}.");
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on updating '{EntityName}'.");
            return OperationResult<T>.Fail($"An error occurred while updating '{EntityName}'.");
        }

        OperationResult<T> addedEtityRes = await GetByIdAsync(entity.Id!.Value);
        return addedEtityRes.Succeeded && addedEtityRes.Data != null
            ? addedEtityRes
            : OperationResult<T>.Success(null, $"{EntityName} has been updated successfully, but failed to fetch it, " +
                $"Errors: \r\n'{string.Join("\r\n", addedEtityRes.Errors)}'");
    }

    protected OperationResult<IQueryable<T>> GetQueryableAsync(Expression<Func<T, bool>>? predicate, int? pageQty, int? pageNum)
    {
        IQueryable<T> entitiesQuer = _dbSet;
        if (_includeNavPropsProvider != null)
            entitiesQuer = _includeNavPropsProvider.ApplyIncludesNavigationProps(entitiesQuer);
        if (predicate != null) entitiesQuer = entitiesQuer.Where(predicate);

        if (pageQty != null && pageNum != null) entitiesQuer = entitiesQuer
                .Skip(pageQty.Value * (pageNum.Value - 1)).Take(pageQty.Value);
        entitiesQuer = entitiesQuer.AsNoTracking();

        return OperationResult<IQueryable<T>>.Success(entitiesQuer);
    }

    protected async Task<OperationResult<List<T>>> GetListAsync(Expression<Func<T, bool>>? predicate, int? pageQty, int? pageNum)
    {
        OperationResult<IQueryable<T>> entitiesQuerRes = GetQueryableAsync(predicate, pageQty, pageNum);
        if (!entitiesQuerRes.Succeeded || entitiesQuerRes.Data == null)
        {
            entitiesQuerRes.AddError($"Failed to retrieve entities of '{EntityName}'.");
            return OperationResult<List<T>>.Fail(entitiesQuerRes.Errors);
        }

        List<T> entities;
        try
        {
            entities = await entitiesQuerRes.Data.ToListAsync();
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on retrieving entities of '{EntityName}'.");
            return OperationResult<List<T>>.Fail($"An error occurred while retrieving records of '{EntityName}'.");
        }

        return OperationResult<List<T>>.Success(entities);
    }

    protected void LogError(string layer, Exception ex, string customMsg)
    {
        _logger.LogError(layer, ex, customMsg);
    }

    protected OperationResult<Expression<Func<T, bool>>> CombineAndTExpressions(
        Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
    {
        Expression<Func<T, bool>> finalExpr;
        try
        {
            finalExpr = _expressionCombiner.And(expr1, expr2);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on combining two expressions of '{EntityName}'.");
            return OperationResult<Expression<Func<T, bool>>>.Fail(
                $"An error occurred while combining two expressions of '{EntityName}'.");
        }
        return OperationResult<Expression<Func<T, bool>>>.Success(finalExpr);
    }

}
