using AutoMapper;
using Helper.DataContainers;
using Helper.Interfaces;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_DAL.Repositories.Interfaces;
using System.Linq.Expressions;

namespace POS_Server_BLL.Implementations.BaseImplementations;

public abstract class HardDeletableService<TModel> : BaseService<TModel> where TModel : BaseModel
{
    protected readonly new IHardDeletableRepository<TModel> _repository;

    public HardDeletableService(IHardDeletableRepository<TModel> repository, IEntityValidator<TModel> validator,
        IMapper mapper, ICustomLogger logger, IUserService userService) : base(repository, validator, mapper, logger, userService)
    {
        _repository = repository;
    }

    public virtual async Task<OperationResult<int>> CountAsync()
    {
        return await _repository.CountAsync();
    }

    public virtual async Task<OperationResult<List<TModel>>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public virtual async Task<OperationResult<List<TModel>>> FilterByAsync(Expression<Func<TModel, bool>> predicate)
    {
        return await _repository.FilterByAsync(predicate);
    }

    public virtual async Task<OperationResult<List<TModel>>> GetAllPagedAsync(int pageQty, int pageNum)
    {
        if (pageQty <= 0 || pageNum <= 0)
            return OperationResult<List<TModel>>.Fail("Page quantity and Page number must be greater than 0.");
        return await _repository.GetAllPagedAsync(pageQty, pageNum);
    }

    public virtual async Task<OperationResult<List<TModel>>> FilterByPagedAsync(Expression<Func<TModel, bool>> predicate, int pageQty, int pageNum)
    {
        if (pageQty <= 0 || pageNum <= 0)
            return OperationResult<List<TModel>>.Fail("Page quantity and Page number must be greater than 0.");
        return await _repository.FilterByPagedAsync(predicate, pageQty, pageNum);
    }

    public virtual async Task<OperationResult<int>> HardDeleteAsync(int entityId, string currUserId)
    {
        if (entityId <= 0) return OperationResult<int>.Fail("Id can't be less than or equal to 0.");

        OperationResult ChkResult = await CheckCurrUser(currUserId);
        if (!ChkResult.Succeeded) return OperationResult<int>.Fail(ChkResult.Errors);

        return await _repository.HardDeleteAsync(entityId);
    }
}
