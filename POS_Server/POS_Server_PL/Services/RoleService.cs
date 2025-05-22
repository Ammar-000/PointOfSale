using POS_Server_BLL.Interfaces.OtherInterfaces;
using Microsoft.AspNetCore.Identity;
using Helper.DataContainers;
using POS_Domains.Models;
using System.Linq.Expressions;
using POS_Server_DAL.Models;
using AutoMapper;
using Helper.Interfaces;
using Microsoft.EntityFrameworkCore;
using POS_Domains.DTOs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace POS_Server_PL.Services;

public class RoleService : IRoleService
{
    readonly RoleManager<ApplicationRoleModel> _roleManager;
    readonly IUserService _userService;
    readonly IExpressionCombiner<ApplicationRoleModel> _expressionCombiner;
    readonly IExpressionConverter<RoleModel, ApplicationRoleModel> _expressionConverter;
    readonly IEntityValidator<RoleModel> _validator;
    readonly IMapper _mapper;
    readonly ICustomLogger _logger;

    string LayerName => "PL";

    public RoleService(RoleManager<ApplicationRoleModel> roleManager, IUserService userService,
        IExpressionCombiner<ApplicationRoleModel> expressionCombiner, IExpressionConverter<RoleModel, ApplicationRoleModel> expressionConverter,
        IEntityValidator<RoleModel> validator, IMapper mapper, ICustomLogger logger)
    {
        _roleManager = roleManager;
        _userService = userService;
        _expressionCombiner = expressionCombiner;
        _expressionConverter = expressionConverter;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    #region Basic Methods

    public async Task<OperationResult<int>> CountAsync()
    {
        try
        {
            return OperationResult<int>.Success(await _roleManager.Roles.CountAsync());
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on retrieving count of Roles.");
            return OperationResult<int>.Fail($"An error occurred while retrieving count of Roles.");
        }
    }

    public async Task<OperationResult<List<RoleModel>>> GetAllAsync()
    {
        return await GetAsync(null, null, null);
    }

    public async Task<OperationResult<List<RoleModel>>> FilterByAsync(Expression<Func<RoleModel, bool>> predicate)
    {
        OperationResult<Expression<Func<ApplicationRoleModel, bool>>> appRolePredRes = ConvertRoleModExToAppRoleModEx(predicate);
        if (!appRolePredRes.Succeeded || appRolePredRes.Data == null)
        {
            appRolePredRes.AddError("Failed to filter roles.");
            return OperationResult<List<RoleModel>>.Fail(appRolePredRes.Errors);
        }
        return await GetAsync(appRolePredRes.Data, null, null);
    }

    public async Task<OperationResult<List<RoleModel>>> GetAllPagedAsync(int pageQty, int pageNum)
    {
        if (pageQty <= 0 || pageNum <= 0)
            return OperationResult<List<RoleModel>>.Fail("Page quantity and Page number must be greater than 0.");
        return await GetAsync(null, pageQty, pageNum);
    }

    public async Task<OperationResult<List<RoleModel>>> FilterByPagedAsync(
        Expression<Func<RoleModel, bool>> predicate, int pageQty, int pageNum)
    {
        if (pageQty <= 0 || pageNum <= 0)
            return OperationResult<List<RoleModel>>.Fail("Page quantity and Page number must be greater than 0.");

        OperationResult<Expression<Func<ApplicationRoleModel, bool>>> appRolePredRes =
            ConvertRoleModExToAppRoleModEx(predicate);
        if (!appRolePredRes.Succeeded || appRolePredRes.Data == null)
        {
            appRolePredRes.AddError("Failed to filter roles.");
            return OperationResult<List<RoleModel>>.Fail(appRolePredRes.Errors);
        }

        return await GetAsync(appRolePredRes.Data, pageQty, pageNum);
    }

    public async Task<OperationResult> CheckExistByAsync(Expression<Func<RoleModel, bool>> predicate)
    {
        OperationResult<Expression<Func<ApplicationRoleModel, bool>>> appRolePredRes = ConvertRoleModExToAppRoleModEx(predicate);
        if (!appRolePredRes.Succeeded || appRolePredRes.Data == null)
        {
            appRolePredRes.AddError("Failed to check role existance.");
            return OperationResult<List<RoleModel>>.Fail(appRolePredRes.Errors);
        }

        Expression<Func<ApplicationRoleModel, bool>> appRolePredicate = appRolePredRes.Data;
        bool exists;
        try
        {
            exists = await _roleManager.Roles.AnyAsync(appRolePredicate);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on checking existance of a role.");
            return OperationResult.Fail($"An error occurred while checking existance of a role.");
        }
        return exists ? OperationResult.Success() : OperationResult.Fail();
    }

    public async Task<OperationResult<RoleModel>> GetByIdAsync(string id)
    {
        RoleModel? role = null;
        try
        {
            ApplicationRoleModel? appRole = await _roleManager.FindByIdAsync(id);
            if (appRole != null) role = _mapper.Map<ApplicationRoleModel, RoleModel>(appRole);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on getting role by Id.");
            return OperationResult<RoleModel>.Fail($"An error occurred while retrieving the role by Id'.");
        }

        return role == null
            ? OperationResult<RoleModel>.Fail($"Role with id '{id}' is not found.")
            : OperationResult<RoleModel>.Success(role);
    }

    public async Task<OperationResult<RoleModel>> AddAsync(RoleModel role, string currUserId)
    {
        OperationResult ChkResult = await PrepareToAddUpdate(role, currUserId, true);
        if (!ChkResult.Succeeded) return OperationResult<RoleModel>.Fail(ChkResult.Errors);

        IdentityResult result;
        try
        {
            ApplicationRoleModel appRole = _mapper.Map<RoleModel, ApplicationRoleModel>(role);
            result = await _roleManager.CreateAsync(appRole);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on creating a new role.");
            return OperationResult<RoleModel>.Fail($"An error occurred while creating a new role.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { "Failed to add new role." };
            errors.AddRange(result.Errors.Select(er => $"Code: {er.Code}, Description: {er.Description}"));
            return OperationResult<RoleModel>.Fail(errors);
        }

        OperationResult<List<RoleModel>> fetchRes = await FilterByAsync(r => r.Name == role.Name && r.CreatedAt == role.CreatedAt);
        if (!fetchRes.Succeeded || fetchRes.Data == null || fetchRes.Data.Count == 0)
            return OperationResult<RoleModel>.Success(null, "Role added successfully, but failed to fetch it.");
        if (fetchRes.Data.Count > 1)
            return OperationResult<RoleModel>.Success(fetchRes.Data[0],
                "Role added successfully, but found more than role with same Name and CreatedAt, one of them returned.");

        return OperationResult<RoleModel>.Success(fetchRes.Data[0]);
    }

    public async Task<OperationResult<RoleModel>> UpdateAsync(RoleModel role, string currUserId)
    {
        if (role == null) return OperationResult<RoleModel>.Fail("Role can't be null.");

        OperationResult ChkResult = await PrepareToAddUpdate(role, currUserId, false);
        if (!ChkResult.Succeeded) return OperationResult<RoleModel>.Fail(ChkResult.Errors);

        IdentityResult result;
        try
        {
            ApplicationRoleModel? appRole = await _roleManager.FindByIdAsync(role.Id);
            if (appRole == null) return OperationResult<RoleModel>.Fail("Failed to fetch original Role.");
            appRole = _mapper.Map(role, appRole);
            result = await _roleManager.UpdateAsync(appRole);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on updating role.");
            return OperationResult<RoleModel>.Fail($"An error occurred while updating role.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { "Failed to update role." };
            errors.AddRange(result.Errors.Select(er => $"Code: {er.Code}, Description: {er.Description}"));
            return OperationResult<RoleModel>.Fail(errors);
        }

        OperationResult<RoleModel> updRoleRes = await GetByIdAsync(role.Id);
        return (updRoleRes.Succeeded && updRoleRes.Data != null)
            ? updRoleRes
            : OperationResult<RoleModel>.Success(null,
                "Role has been updated successfully, but failed to fetch updated role.\r\n"
                + string.Join("\r\n", updRoleRes.Errors));
    }

    public async Task<OperationResult<string>> HardDeleteAsync(string roleId, string currUserId)
    {
        if (string.IsNullOrWhiteSpace(roleId)) return OperationResult<string>.Fail("Id can't be empty.");

        OperationResult ChkResult = await CheckCurrUser(currUserId);
        if (!ChkResult.Succeeded) return OperationResult<string>.Fail(ChkResult.Errors);

        IdentityResult result;
        try
        {
            ApplicationRoleModel? appRole = await _roleManager.FindByIdAsync(roleId);
            if (appRole == null) return OperationResult<string>.Fail("Failed to fetch Role.");
            result = await _roleManager.DeleteAsync(appRole);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on deleting role.");
            return OperationResult<string>.Fail($"An error occurred while deleting role.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { "Failed to delete role." };
            errors.AddRange(result.Errors.Select(er => $"Code: {er.Code}, Description: {er.Description}"));
            return OperationResult<string>.Fail(errors);
        }

        return OperationResult<string>.Success(roleId);
    }


    #endregion

    #region Helper methods

    async Task<OperationResult<List<RoleModel>>> GetAsync(Expression<Func<ApplicationRoleModel, bool>>? predicate, int? pageQty, int? pageNum)
    {
        IQueryable<ApplicationRoleModel> appRoleQuer = predicate == null ?
            _roleManager.Roles : _roleManager.Roles.Where(predicate);
        if (pageQty != null && pageNum != null) appRoleQuer = appRoleQuer
                .Skip(pageQty.Value * (pageNum.Value - 1)).Take(pageQty.Value);

        List<RoleModel> roles;
        try
        {
            List<ApplicationRoleModel> appRoles = await appRoleQuer.ToListAsync();
            roles = _mapper.Map<List<ApplicationRoleModel>, List<RoleModel>>(appRoles);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, "Error on retrieving entities of roles.");
            return OperationResult<List<RoleModel>>.Fail("An error occurred while retrieving records of roles.");
        }
        return OperationResult<List<RoleModel>>.Success(roles);
    }

    async Task<OperationResult> PrepareToAddUpdate(RoleModel role, string currUserId, bool isNew)
    {
        if (role == null) return OperationResult.Fail("Role can't be null.");

        OperationResult ChkResult = await CheckCurrUser(currUserId);
        if (!ChkResult.Succeeded) return ChkResult;

        ChkResult = await SetCreatedProps(role, currUserId, isNew);
        if (!ChkResult.Succeeded) return ChkResult;

        SetUpdatedProps(role, currUserId, isNew);

        ChkResult = ValidateEntity(role);
        if (!ChkResult.Succeeded) return ChkResult;

        return OperationResult.Success();
    }

    OperationResult<Expression<Func<ApplicationRoleModel, bool>>> CombineAndAppRoleModEx(
        Expression<Func<ApplicationRoleModel, bool>> expr1, Expression<Func<ApplicationRoleModel, bool>> expr2)
    {
        Expression<Func<ApplicationRoleModel, bool>> finalExpr;
        try
        {
            finalExpr = _expressionCombiner.And(expr1, expr2);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, "Error on combining two ApplicationRoleModel expressions.");
            return OperationResult<Expression<Func<ApplicationRoleModel, bool>>>.Fail(
                $"An error occurred while combining two ApplicationRoleModel expressions.");
        }
        return OperationResult<Expression<Func<ApplicationRoleModel, bool>>>.Success(finalExpr);
    }

    OperationResult<Expression<Func<ApplicationRoleModel, bool>>> ConvertRoleModExToAppRoleModEx(Expression<Func<RoleModel, bool>> rolePredicate)
    {
        Expression<Func<ApplicationRoleModel, bool>> appRolePredicate;
        try
        {
            appRolePredicate = _expressionConverter.Convert(rolePredicate);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, "Error on converting RoleModel expression to ApplicationRoleModel expression.");
            return OperationResult<Expression<Func<ApplicationRoleModel, bool>>>.Fail(
                $"An error occurred while converting RoleModel expression to ApplicationRoleModel expression.");
        }
        return OperationResult<Expression<Func<ApplicationRoleModel, bool>>>.Success(appRolePredicate);
    }

    async Task<OperationResult> CheckCurrUser(string currUserId)
    {
        if (string.IsNullOrWhiteSpace(currUserId)) return OperationResult.Fail("Current role Id can't be empty");

        OperationResult userExistRes = await _userService.CheckExistByAsync(u => u.Id == currUserId);
        if (!userExistRes.Succeeded)
        {
            userExistRes.Errors.Add($"There is no user with id '{currUserId}'.");
            return OperationResult.Fail(userExistRes.Errors);
        }

        return OperationResult.Success();
    }

    async Task<OperationResult> SetCreatedProps(RoleModel role, string currUserId, bool isRoleNew)
    {
        if (isRoleNew)
        {
            role.CreatedBy = currUserId;
            role.CreatedAt = DateTime.Now;
            return OperationResult.Success();
        }

        OperationResult<RoleModel> fetchResult = await GetByIdAsync(role.Id);
        if (!fetchResult.Succeeded || fetchResult.Data == null)
        {
            fetchResult.AddError($"Failed to fetch original role.");
            return OperationResult.Fail(fetchResult.Errors);
        }

        role.CreatedBy = fetchResult.Data.CreatedBy;
        role.CreatedAt = fetchResult.Data.CreatedAt;

        return OperationResult.Success();
    }

    void SetUpdatedProps(RoleModel role, string currUserId, bool isRoleNew)
    {
        if (isRoleNew)
        {
            role.UpdatedBy = null;
            role.UpdatedAt = null;
            return;
        }

        role.UpdatedBy = currUserId;
        role.UpdatedAt = DateTime.Now;
    }

    OperationResult ValidateEntity(RoleModel role)
    {
        if (role == null) return OperationResult.Fail("Role can't be null.");
        return _validator.ValidateEntity(role);
    }

    protected void LogError(string layer, Exception ex, string customMsg)
    {
        _logger.LogError(layer, ex, customMsg);
    }


    #endregion

    #region Map mthods

    public OperationResult<RoleModel> MapModelDTO(RoleDTO dto)
    {
        string failMsg = $"Failed to map RoleDTO to RoleModel.";

        if (dto == null) return OperationResult<RoleModel>.Fail(failMsg, "dto can't be null.");
        RoleModel? model;

        try
        {
            model = _mapper.Map<RoleDTO, RoleModel>(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, failMsg);
            return OperationResult<RoleModel>.Fail(failMsg);
        }

        return OperationResult<RoleModel>.Success(model);
    }

    public OperationResult<RoleDTO> MapModelDTO(RoleModel model)
    {
        string failMsg = $"Failed to map RoleModel to RoleDTO.";

        if (model == null) return OperationResult<RoleDTO>.Fail(failMsg, "Model can't be null.");
        RoleDTO? dto;

        try
        {
            dto = _mapper.Map<RoleModel, RoleDTO>(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, failMsg);
            return OperationResult<RoleDTO>.Fail(failMsg);
        }

        return OperationResult<RoleDTO>.Success(dto);
    }

    public OperationResult<List<RoleModel>> MapModelDTO(List<RoleDTO> dtos)
    {
        string failMsg = $"Failed to map List<RoleDTO> to List<RoleModel>.";

        if (dtos == null) return OperationResult<List<RoleModel>>.Fail(failMsg, "dtos can't be null.");
        if (dtos.Count == 0) return OperationResult<List<RoleModel>>.Success(new());
        List<RoleModel> models;

        try
        {
            models = _mapper.Map<List<RoleDTO>, List<RoleModel>>(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, failMsg);
            return OperationResult<List<RoleModel>>.Fail(failMsg);
        }

        return OperationResult<List<RoleModel>>.Success(models);
    }

    public OperationResult<List<RoleDTO>> MapModelDTO(List<RoleModel> models)
    {
        string failMsg = $"Failed to map List<RoleModel> to List<RoleDTO>.";

        if (models == null) return OperationResult<List<RoleDTO>>.Fail(failMsg, "Model can't be null.");
        if (models.Count == 0) return OperationResult<List<RoleDTO>>.Success(new());
        List<RoleDTO> dtos;

        try
        {
            dtos = _mapper.Map<List<RoleModel>, List<RoleDTO>>(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, failMsg);
            return OperationResult<List<RoleDTO>>.Fail(failMsg);
        }

        return OperationResult<List<RoleDTO>>.Success(dtos);
    }

    public OperationResult<RoleModel> MapModelDTO(OperationResult<RoleDTO> dtoRes)
    {
        OperationResult<RoleModel> modelRes = dtoRes.Succeeded
            ? OperationResult<RoleModel>.Success(null) : OperationResult<RoleModel>.Fail(dtoRes.Errors);
        modelRes.Message = dtoRes.Message;
        modelRes.Errors = dtoRes.Errors;
        if (dtoRes.Data == null) return modelRes;

        OperationResult<RoleModel> dataDtoToModRes = MapModelDTO(dtoRes.Data);
        if (dataDtoToModRes.Succeeded && dataDtoToModRes.Data != null)
        {
            modelRes.Data = dataDtoToModRes.Data;
            return modelRes;
        }

        modelRes.Errors.AddRange(dataDtoToModRes.Errors);
        modelRes.AddError($"Failed to map OperationResult<RoleDTO> to OperationResult<RoleModel>.");

        return modelRes;
    }

    public OperationResult<RoleDTO> MapModelDTO(OperationResult<RoleModel> modelRes)
    {
        OperationResult<RoleDTO> dtoRes = modelRes.Succeeded
            ? OperationResult<RoleDTO>.Success(null) : OperationResult<RoleDTO>.Fail(modelRes.Errors);
        dtoRes.Message = modelRes.Message;
        dtoRes.Errors = modelRes.Errors;
        if (modelRes.Data == null) return dtoRes;

        OperationResult<RoleDTO> dataModToDtoRes = MapModelDTO(modelRes.Data);
        if (dataModToDtoRes.Succeeded && dataModToDtoRes.Data != null)
        {
            dtoRes.Data = dataModToDtoRes.Data;
            return dtoRes;
        }

        dtoRes.Errors.AddRange(dataModToDtoRes.Errors);
        dtoRes.AddError($"Failed to map OperationResult<RoleModel> to OperationResult<RoleDTO>.");

        return dtoRes;
    }

    public OperationResult<List<RoleModel>> MapModelDTO(OperationResult<List<RoleDTO>> dtosRes)
    {
        OperationResult<List<RoleModel>> modelsRes = dtosRes.Succeeded
            ? OperationResult<List<RoleModel>>.Success(null) : OperationResult<List<RoleModel>>.Fail(dtosRes.Errors);
        modelsRes.Message = dtosRes.Message;
        modelsRes.Errors = dtosRes.Errors;
        if (dtosRes.Data == null) return modelsRes;
        if (dtosRes.Data.Count == 0)
        {
            modelsRes.Data = new();
            return modelsRes;
        }

        OperationResult<List<RoleModel>> dataDtoToModRes = MapModelDTO(dtosRes.Data);
        if (dataDtoToModRes.Succeeded && dataDtoToModRes.Data != null)
        {
            modelsRes.Data = dataDtoToModRes.Data;
            return modelsRes;
        }

        modelsRes.Errors.AddRange(dataDtoToModRes.Errors);
        modelsRes.AddError($"Failed to map OperationResult<List<RoleDTO>> to OperationResult<List<RoleModel>>.");

        return modelsRes;
    }

    public OperationResult<List<RoleDTO>> MapModelDTO(OperationResult<List<RoleModel>> modelsRes)
    {
        OperationResult<List<RoleDTO>> dtosRes = modelsRes.Succeeded
            ? OperationResult<List<RoleDTO>>.Success(null) : OperationResult<List<RoleDTO>>.Fail(modelsRes.Errors);
        dtosRes.Message = modelsRes.Message;
        dtosRes.Errors = modelsRes.Errors;
        if (modelsRes.Data == null) return dtosRes;
        if (modelsRes.Data.Count == 0)
        {
            dtosRes.Data = new();
            return dtosRes;
        }

        OperationResult<List<RoleDTO>> dataModToDtoRes = MapModelDTO(modelsRes.Data);
        if (dataModToDtoRes.Succeeded && dataModToDtoRes.Data != null)
        {
            dtosRes.Data = dataModToDtoRes.Data;
            return dtosRes;
        }

        dtosRes.Errors.AddRange(dataModToDtoRes.Errors);
        dtosRes.AddError($"Failed to map OperationResult<List<RoleModel>> to OperationResult<List<RoleDTO>>.");

        return dtosRes;
    }

    #endregion

}
