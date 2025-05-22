using AutoMapper;
using Helper.DataContainers;
using Helper.Interfaces;
using POS_Domains.DTOs;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_DAL.Repositories.Interfaces;

namespace POS_Server_BLL.Implementations.BaseImplementations;

public abstract class BaseService<TModel> where TModel : BaseModel
{
    protected readonly IBaseRepository<TModel> _repository;
    protected readonly IEntityValidator<TModel> _validator;
    protected readonly IMapper _mapper;
    protected readonly ICustomLogger _logger;
    protected readonly IUserService _userService;

    protected string LayerName => "BLL";
    protected string EntityName => typeof(TModel).Name;
    protected string ModelName => typeof(TModel).Name;

    protected BaseService(IBaseRepository<TModel> repository, IEntityValidator<TModel> validator,
        IMapper mapper, ICustomLogger logger, IUserService userService)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
        _validator = validator;
        _userService = userService;
    }

    #region CRUD methods

    public virtual async Task<OperationResult<TModel>> GetByIdAsync(int id)
    {
        if (id <= 0) return OperationResult<TModel>.Fail("Id can't be less than or equal to 0.");
        return await _repository.GetByIdAsync(id);
    }

    public virtual async Task<OperationResult<TModel>> AddAsync(TModel entity, string currUserId)
    {
        OperationResult ChkResult = await PrepareToAddUpdate(entity, currUserId, true);
        if (!ChkResult.Succeeded) return OperationResult<TModel>.Fail(ChkResult.Errors);

        return await _repository.AddAsync(entity);
    }

    public virtual async Task<OperationResult<TModel>> UpdateAsync(TModel entity, string currUserId)
    {
        OperationResult ChkResult = await PrepareToAddUpdate(entity, currUserId, false);
        if (!ChkResult.Succeeded) return OperationResult<TModel>.Fail(ChkResult.Errors);

        return await _repository.UpdateAsync(entity);
    }

    #endregion

    #region Helper methods

    protected async Task<OperationResult> CheckCurrUser(string currUserId)
    {
        if (string.IsNullOrWhiteSpace(currUserId)) return OperationResult.Fail("Current user Id can't be empty");

        OperationResult userExistRes = await _userService.CheckExistByAsync(u => u.Id == currUserId);
        if (!userExistRes.Succeeded)
        {
            userExistRes.Errors.Add($"There is no user with id '{currUserId}'.");
            return OperationResult.Fail(userExistRes.Errors);
        }

        return OperationResult.Success();
    }

    protected virtual async Task<OperationResult> SetCreatedProps(TModel entity, string currUserId, bool isEntityNew)
    {
        if (isEntityNew)
        {
            entity.CreatedBy = currUserId;
            entity.CreatedAt = DateTime.Now;
            return OperationResult.Success();
        }

        if (entity.Id == null) return OperationResult.Fail("Id can't be null.");
        OperationResult<TModel> fetchResult = await GetByIdAsync(entity.Id.Value);
        if (!fetchResult.Succeeded || fetchResult.Data == null)
        {
            fetchResult.AddError($"Failed to feach original entity {EntityName}.");
            return OperationResult.Fail(fetchResult.Errors);
        }

        entity.CreatedBy = fetchResult.Data.CreatedBy;
        entity.CreatedAt = fetchResult.Data.CreatedAt;

        return OperationResult.Success();
    }

    protected virtual void SetUpdatedProps(TModel entity, string currUserId, bool isNew)
    {
        if (isNew)
        {
            entity.UpdatedBy = null;
            entity.UpdatedAt = null;
            return;
        }

        entity.UpdatedBy = currUserId;
        entity.UpdatedAt = DateTime.Now;
    }

    protected OperationResult ValidateEntity(TModel entity)
    {
        if (entity == null) return OperationResult.Fail("Entity can't be null.");
        return _validator.ValidateEntity(entity);
    }

    async Task<OperationResult> PrepareToAddUpdate(TModel entity, string currUserId, bool isNew)
    {
        if (entity == null) return OperationResult<List<TModel>>.Fail("Entity can't be null.");

        RemoveNavigationProps(entity);

        OperationResult ChkResult = await CheckCurrUser(currUserId);
        if (!ChkResult.Succeeded) return ChkResult;

        SetUpdatedProps(entity, currUserId, isNew);

        ChkResult = await SetCreatedProps(entity, currUserId, isNew);
        if (!ChkResult.Succeeded) return ChkResult;

        ChkResult = ValidateEntity(entity);
        if (!ChkResult.Succeeded) return ChkResult;

        if (isNew) entity.Id = default;
        return OperationResult.Success();
    }

    public abstract void RemoveNavigationProps(TModel entity);

    protected void LogError(string layer, Exception ex, string customMsg)
    {
        _logger.LogError(layer, ex, customMsg);
    }

    #endregion

    #region Map mthods

    public OperationResult<TModel> MapModelDTO<TDTO>(TDTO dto) where TDTO : BaseDTO
    {
        string DTOName = typeof(TDTO).Name;
        string failMsg = $"Failed to map {DTOName} to {ModelName}.";

        if (dto == null) return OperationResult<TModel>.Fail(failMsg, "dto can't be null.");
        TModel? model;

        try
        {
            model = _mapper.Map<TDTO, TModel>(dto);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, failMsg);
            return OperationResult<TModel>.Fail(failMsg);
        }

        return OperationResult<TModel>.Success(model);
    }

    public OperationResult<TDTO> MapModelDTO<TDTO>(TModel model) where TDTO : BaseDTO
    {
        string DTOName = typeof(TDTO).Name;
        string failMsg = $"Failed to map {ModelName} to {DTOName}.";

        if (model == null) return OperationResult<TDTO>.Fail(failMsg, "Model can't be null.");
        TDTO? dto;

        try
        {
            dto = _mapper.Map<TModel, TDTO>(model);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, failMsg);
            return OperationResult<TDTO>.Fail(failMsg);
        }

        return OperationResult<TDTO>.Success(dto);
    }

    public OperationResult<List<TModel>> MapModelDTO<TDTO>(List<TDTO> dtos) where TDTO : BaseDTO
    {
        string DTOName = typeof(TDTO).Name;
        string failMsg = $"Failed to map List<{DTOName}> to List<{ModelName}>.";

        if (dtos == null) return OperationResult<List<TModel>>.Fail(failMsg, "dtos can't be null.");
        if (dtos.Count == 0) return OperationResult<List<TModel>>.Success(new());
        List<TModel> models;

        try
        {
            models = _mapper.Map<List<TDTO>, List<TModel>>(dtos);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, failMsg);
            return OperationResult<List<TModel>>.Fail(failMsg);
        }

        return OperationResult<List<TModel>>.Success(models);
    }

    public OperationResult<List<TDTO>> MapModelDTO<TDTO>(List<TModel> models) where TDTO : BaseDTO
    {
        string DTOName = typeof(TDTO).Name;
        string failMsg = $"Failed to map List<{ModelName}> to List<{DTOName}>.";

        if (models == null) return OperationResult<List<TDTO>>.Fail(failMsg, "Model can't be null.");
        if (models.Count == 0) return OperationResult<List<TDTO>>.Success(new());
        List<TDTO> dtos;

        try
        {
            dtos = _mapper.Map<List<TModel>, List<TDTO>>(models);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, failMsg);
            return OperationResult<List<TDTO>>.Fail(failMsg);
        }

        return OperationResult<List<TDTO>>.Success(dtos);
    }

    public OperationResult<TModel> MapModelDTO<TDTO>(OperationResult<TDTO> dtoRes) where TDTO : BaseDTO
    {
        OperationResult<TModel> modelRes = dtoRes.Succeeded
            ? OperationResult<TModel>.Success(null) : OperationResult<TModel>.Fail(dtoRes.Errors);
        modelRes.Message = dtoRes.Message;
        modelRes.Errors = dtoRes.Errors;
        if (dtoRes.Data == null) return modelRes;

        OperationResult<TModel> dataDtoToModRes = MapModelDTO(dtoRes.Data);
        if (dataDtoToModRes.Succeeded && dataDtoToModRes.Data != null)
        {
            modelRes.Data = dataDtoToModRes.Data;
            return modelRes;
        }

        modelRes.Errors.AddRange(dataDtoToModRes.Errors);
        string DTOName = typeof(TDTO).Name;
        modelRes.AddError($"Failed to map OperationResult<{DTOName}> to OperationResult<{ModelName}>.");

        return modelRes;
    }

    public OperationResult<TDTO> MapModelDTO<TDTO>(OperationResult<TModel> modelRes) where TDTO : BaseDTO
    {
        OperationResult<TDTO> dtoRes = modelRes.Succeeded
            ? OperationResult<TDTO>.Success(null) : OperationResult<TDTO>.Fail(modelRes.Errors);
        dtoRes.Message = modelRes.Message;
        dtoRes.Errors = modelRes.Errors;
        if (modelRes.Data == null) return dtoRes;

        OperationResult<TDTO> dataModToDtoRes = MapModelDTO<TDTO>(modelRes.Data);
        if (dataModToDtoRes.Succeeded && dataModToDtoRes.Data != null)
        {
            dtoRes.Data = dataModToDtoRes.Data;
            return dtoRes;
        }

        dtoRes.Errors.AddRange(dataModToDtoRes.Errors);
        string DTOName = typeof(TDTO).Name;
        dtoRes.AddError($"Failed to map OperationResult<{ModelName}> to OperationResult<{DTOName}>.");

        return dtoRes;
    }

    public OperationResult<List<TModel>> MapModelDTO<TDTO>(OperationResult<List<TDTO>> dtosRes) where TDTO : BaseDTO
    {
        OperationResult<List<TModel>> modelsRes = dtosRes.Succeeded
            ? OperationResult<List<TModel>>.Success(null) : OperationResult<List<TModel>>.Fail(dtosRes.Errors);
        modelsRes.Message = dtosRes.Message;
        modelsRes.Errors = dtosRes.Errors;
        if (dtosRes.Data == null) return modelsRes;
        if (dtosRes.Data.Count == 0)
        {
            modelsRes.Data = new();
            return modelsRes;
        }

        OperationResult<List<TModel>> dataDtoToModRes = MapModelDTO(dtosRes.Data);
        if (dataDtoToModRes.Succeeded && dataDtoToModRes.Data != null)
        {
            modelsRes.Data = dataDtoToModRes.Data;
            return modelsRes;
        }

        modelsRes.Errors.AddRange(dataDtoToModRes.Errors);
        string DTOName = typeof(TDTO).Name;
        modelsRes.AddError($"Failed to map OperationResult<List<{DTOName}>> to OperationResult<List<{ModelName}>>.");

        return modelsRes;
    }

    public OperationResult<List<TDTO>> MapModelDTO<TDTO>(OperationResult<List<TModel>> modelsRes) where TDTO : BaseDTO
    {
        OperationResult<List<TDTO>> dtosRes = modelsRes.Succeeded
            ? OperationResult<List<TDTO>>.Success(null) : OperationResult<List<TDTO>>.Fail(modelsRes.Errors);
        dtosRes.Message = modelsRes.Message;
        dtosRes.Errors = modelsRes.Errors;
        if (modelsRes.Data == null) return dtosRes;
        if (modelsRes.Data.Count == 0)
        {
            dtosRes.Data = new();
            return dtosRes;
        }

        OperationResult<List<TDTO>> dataModToDtoRes = MapModelDTO<TDTO>(modelsRes.Data);
        if (dataModToDtoRes.Succeeded && dataModToDtoRes.Data != null)
        {
            dtosRes.Data = dataModToDtoRes.Data;
            return dtosRes;
        }

        dtosRes.Errors.AddRange(dataModToDtoRes.Errors);
        string DTOName = typeof(TDTO).Name;
        dtosRes.AddError($"Failed to map OperationResult<List<{ModelName}>> to OperationResult<List<{DTOName}>>.");

        return dtosRes;
    }

    #endregion

}