using AutoMapper;
using Helper.DataContainers;
using Helper.Interfaces;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_DAL.Repositories.Interfaces;
using System.Linq.Expressions;

namespace POS_Server_BLL.Implementations.BaseImplementations;

public abstract class SoftDeletableService<TModel> : BaseService<TModel> where TModel : BaseSoftDeletableModel
{
    protected readonly new ISoftDeletableRepository<TModel> _repository;

    protected new string EntityName => typeof(TModel).Name;

    public SoftDeletableService(ISoftDeletableRepository<TModel> repository, IEntityValidator<TModel> validator,
        IMapper mapper, ICustomLogger logger, IUserService userService) : base(repository, validator, mapper, logger, userService)
    {
        _repository = repository;
    }

    public virtual async Task<OperationResult<int>> CountAsync(bool includeInActive = false)
    {
        return await _repository.CountAsync(includeInActive);
    }

    public virtual async Task<OperationResult<List<TModel>>> GetAllAsync(bool includeInActive = false)
    {
        return await _repository.GetAllAsync(includeInActive);
    }

    public virtual async Task<OperationResult<List<TModel>>> FilterByAsync(Expression<Func<TModel, bool>> predicate, bool includeInActive = false)
    {
        return await _repository.FilterByAsync(predicate, includeInActive);
    }

    public virtual async Task<OperationResult<List<TModel>>> GetAllPagedAsync(int pageQty, int pageNum, bool includeInActive = false)
    {
        if (pageQty <= 0 || pageNum <= 0)
            return OperationResult<List<TModel>>.Fail("Page quantity and Page number must be greater than 0.");
        return await _repository.GetAllPagedAsync(pageQty, pageNum, includeInActive);
    }

    public virtual async Task<OperationResult<List<TModel>>> FilterByPagedAsync(Expression<Func<TModel, bool>> predicate,
        int pageQty, int pageNum, bool includeInActive = false)
    {
        if (pageQty <= 0 || pageNum <= 0)
            return OperationResult<List<TModel>>.Fail("Page quantity and Page number must be greater than 0.");
        return await _repository.FilterByPagedAsync(predicate, pageQty, pageNum, includeInActive);
    }

    public override Task<OperationResult<TModel>> AddAsync(TModel entity, string currUserId)
    {
        entity.IsActive = true;
        return base.AddAsync(entity, currUserId);
    }

    public override async Task<OperationResult<TModel>> UpdateAsync(TModel entity, string currUserId)
    {
        if (entity == null) return OperationResult<TModel>.Fail("Entity can't be null.");

        if (entity.Id == null) return OperationResult<TModel>.Fail("Id can't be null.");
        OperationResult<TModel> fetchingResult = await GetByIdAsync(entity.Id.Value);
        if (!fetchingResult.Succeeded || fetchingResult.Data == null)
        {
            fetchingResult.AddError($"Failed to fetch original {EntityName}.");
            return fetchingResult;
        }
        if (!fetchingResult.Data.IsActive)
            return OperationResult<TModel>.Fail($"Failed to update soft-deleted entity {EntityName}, it must be restored first");

        return await base.UpdateAsync(entity, currUserId);
    }

    public virtual async Task<OperationResult<int>> SoftDeleteAsync(int entityId, string currUserId)
    {
        if (entityId <= 0) return OperationResult<int>.Fail("Id can't be less than or equal to 0.");

        OperationResult ChkResult = await CheckCurrUser(currUserId);
        if (!ChkResult.Succeeded) return OperationResult<int>.Fail(ChkResult.Errors);

        return await _repository.SoftDeleteAsync(entityId);
    }

    public async Task<OperationResult<TModel>> RestoreAsync(int entityId, string currUserId)
    {
        if (entityId <= 0) return OperationResult<TModel>.Fail("Id can't be less than or equal to 0.");

        OperationResult ChkResult = await CheckCurrUser(currUserId);
        if (!ChkResult.Succeeded) return OperationResult<TModel>.Fail(ChkResult.Errors);

        return await _repository.RestoreAsync(entityId);
    }
}
