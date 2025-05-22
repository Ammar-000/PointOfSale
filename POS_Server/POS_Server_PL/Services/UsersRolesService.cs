using AutoMapper;
using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Identity;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_DAL.Models;

namespace POS_Server_PL.Services;

public class UsersRolesService : IUsersRolesService
{
    readonly IUserService _userService;
    readonly IRoleService _roleService;
    readonly UserManager<ApplicationUserModel> _userManager;
    readonly IMapper _mapper;
    readonly ICustomLogger _logger;

    string LayerName => "PL";

    public UsersRolesService(IUserService userService, IRoleService roleService,
        UserManager<ApplicationUserModel> userManager, IMapper mapper, ICustomLogger logger)
    {
        _userService = userService;
        _roleService = roleService;
        _userManager = userManager;
        _mapper = mapper;
        _logger = logger;
    }

    #region Basic Methods

    public async Task<OperationResult<List<string>>> GetUserRolesNamesAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult<List<string>>.Fail("UserId can't be empty.");

        List<string> roles;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult<List<string>>.Fail($"There is no user with Id '{userId}'");
            roles = (await _userManager.GetRolesAsync(appUser)).ToList();
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on getting user roles.");
            return OperationResult<List<string>>.Fail($"An error occurred while getting user roles.");
        }

        return OperationResult<List<string>>.Success(roles);
    }

    public async Task<OperationResult<List<UserModel>>> GetUsersInRoleAsync(string roleId)
    {
        if (string.IsNullOrWhiteSpace(roleId)) return OperationResult<List<UserModel>>.Fail("RoleId can't be empty.");

        OperationResult<RoleModel> rolesRes = await _roleService.GetByIdAsync(roleId);
        if (!rolesRes.Succeeded || rolesRes.Data == null) return OperationResult<List<UserModel>>.Fail(rolesRes.Errors);
        List<UserModel> users;
        try
        {
            users = _mapper.Map<List<ApplicationUserModel>, List<UserModel>>(
                (await _userManager.GetUsersInRoleAsync(rolesRes.Data.Name)).ToList());
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on getting users in role.");
            return OperationResult<List<UserModel>>.Fail($"An error occurred while getting users in role.");
        }

        return OperationResult<List<UserModel>>.Success(users);
    }

    public async Task<OperationResult> IsInRoleAsync(string userId, string roleId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult.Fail("UserId can't be empty.");
        if (string.IsNullOrWhiteSpace(roleId)) return OperationResult.Fail("RoleId can't be empty.");

        OperationResult<RoleModel> rolesRes = await _roleService.GetByIdAsync(roleId);
        if (!rolesRes.Succeeded || rolesRes.Data == null) return OperationResult<List<string>>.Fail(rolesRes.Errors);

        bool isInRole;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult.Fail($"There is no user with Id '{userId}'");
            isInRole = await _userManager.IsInRoleAsync(appUser, rolesRes.Data.Name);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on checking user with Id '{userId}' is in role '{roleId}'.");
            return OperationResult.Fail($"An error occurred while checking user with Id '{userId}' is in role with id '{roleId}'.");
        }

        return isInRole ? OperationResult.Success() : OperationResult.Fail($"User with Id '{userId}' isn't in role with id '{roleId}'");
    }

    public async Task<OperationResult> AddToRoleAsync(string userId, string roleId, string currUserId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult.Fail("UserId can't be empty.");
        if (string.IsNullOrWhiteSpace(roleId)) return OperationResult.Fail("RoleId can't be empty.");
        OperationResult chkResult = await CheckCurrUser(currUserId);
        if (!chkResult.Succeeded) return OperationResult.Fail(chkResult.Errors);

        OperationResult<RoleModel> rolesRes = await _roleService.GetByIdAsync(roleId);
        if (!rolesRes.Succeeded || rolesRes.Data == null) return OperationResult<List<string>>.Fail(rolesRes.Errors);

        IdentityResult result;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult.Fail($"There is no user with Id '{userId}'");
            result = await _userManager.AddToRoleAsync(appUser, rolesRes.Data.Name);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on adding user with Id '{userId}' to role with id '{roleId}'.");
            return OperationResult.Fail($"An error occurred while adding user with Id '{userId}' to role with id '{roleId}'.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { $"Failed to add User with Id '{userId}' to Role with id '{roleId}'." };
            errors.AddRange(result.Errors.Select(e => $"Code: {e.Code}, Description: {e.Description}."));
            return OperationResult.Fail(errors);
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> RemoveFromRoleAsync(string userId, string roleId, string currUserId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult.Fail("UserId can't be empty.");
        if (string.IsNullOrWhiteSpace(roleId)) return OperationResult.Fail("RoleId can't be empty.");
        OperationResult chkResult = await CheckCurrUser(currUserId);
        if (!chkResult.Succeeded) return OperationResult.Fail(chkResult.Errors);

        OperationResult<RoleModel> rolesRes = await _roleService.GetByIdAsync(roleId);
        if (!rolesRes.Succeeded || rolesRes.Data == null) return OperationResult<List<string>>.Fail(rolesRes.Errors);

        IdentityResult result;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult.Fail($"There is no user with Id '{userId}'");
            result = await _userManager.RemoveFromRoleAsync(appUser, rolesRes.Data.Name);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on removing user with Id '{userId}' from role with id '{roleId}'.");
            return OperationResult.Fail($"An error occurred while removing user with Id '{userId}' from role with id '{roleId}'.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { $"Failed to remove User with Id '{userId}' from Role with id '{roleId}'." };
            errors.AddRange(result.Errors.Select(e => $"Code: {e.Code}, Description: {e.Description}."));
            return OperationResult.Fail(errors);
        }

        return OperationResult.Success();
    }

    #endregion

    #region Helper methods

    async Task<OperationResult> CheckCurrUser(string? currUserId, bool addingNewUser = false)
    {
        if (string.IsNullOrWhiteSpace(currUserId))
        {
            if (!addingNewUser) return OperationResult.Fail("Current user Id can't be empty");
            if ((await _userService.CheckExistByAsync(u => true, true)).Succeeded)
                return OperationResult.Fail("Current user Id is required since there this is not first User");
            return OperationResult.Success();
        }

        OperationResult userExistRes = await _userService.CheckExistByAsync(u => u.Id == currUserId);
        if (!userExistRes.Succeeded)
        {
            userExistRes.Errors.Add($"There is no user with id '{currUserId}'.");
            return OperationResult.Fail(userExistRes.Errors);
        }

        return OperationResult.Success();
    }

    void LogError(string layer, Exception ex, string customMsg)
    {
        _logger.LogError(layer, ex, customMsg);
    }

    #endregion
}
