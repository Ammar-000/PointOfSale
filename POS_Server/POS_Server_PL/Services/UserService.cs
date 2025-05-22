using AutoMapper;
using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using POS_Domains.DTOs;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_DAL.Models;
using System.Linq.Expressions;

namespace POS_Server_PL.Services;

public class UserService : IUserService
{
    readonly UserManager<ApplicationUserModel> _userManager;
    readonly IExpressionCombiner<ApplicationUserModel> _expressionCombiner;
    readonly IExpressionConverter<UserModel, ApplicationUserModel> _expressionConverter;
    readonly IEntityValidator<UserModel> _validator;
    readonly IMapper _mapper;
    readonly ICustomLogger _logger;

    string LayerName => "PL";

    public UserService(UserManager<ApplicationUserModel> userManager,
        IExpressionCombiner<ApplicationUserModel> expressionCombiner,
        IExpressionConverter<UserModel, ApplicationUserModel> expressionConverter,
        IEntityValidator<UserModel> validator, IMapper mapper, ICustomLogger logger)
    {
        _userManager = userManager;
        _expressionCombiner = expressionCombiner;
        _expressionConverter = expressionConverter;
        _validator = validator;
        _mapper = mapper;
        _logger = logger;
    }

    #region Basic Methods

    public async Task<OperationResult<int>> CountAsync(bool includeInActive = false)
    {
        int count = -1;
        try
        {
            count = includeInActive
                ? await _userManager.Users.CountAsync()
                : await _userManager.Users.CountAsync(u => u.IsActive);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on retrieving count of Users.");
            return OperationResult<int>.Fail($"An error occurred while retrieving count of Users.");
        }
        return OperationResult<int>.Success(count);
    }

    public async Task<OperationResult<List<UserModel>>> GetAllAsync(bool includeInActive = false)
    {
        Expression<Func<ApplicationUserModel, bool>>? predicate = includeInActive ? null : u => u.IsActive;
        return await GetAsync(predicate, null, null);
    }

    public async Task<OperationResult<List<UserModel>>> FilterByAsync(Expression<Func<UserModel, bool>> predicate, bool includeInActive = false)
    {
        OperationResult<Expression<Func<ApplicationUserModel, bool>>> appUserPredRes = ConvertUserModExToAppUserModEx(predicate);
        if (!appUserPredRes.Succeeded || appUserPredRes.Data == null)
        {
            appUserPredRes.AddError("Failed to filter users.");
            return OperationResult<List<UserModel>>.Fail(appUserPredRes.Errors);
        }
        if (!includeInActive) appUserPredRes = CombineAndAppUserModEx(appUserPredRes.Data, u => u.IsActive);
        if (!appUserPredRes.Succeeded || appUserPredRes.Data == null)
        {
            appUserPredRes.AddError("Failed to filter users.");
            return OperationResult<List<UserModel>>.Fail(appUserPredRes.Errors);
        }

        return await GetAsync(appUserPredRes.Data, null, null);
    }

    public async Task<OperationResult<List<UserModel>>> GetAllPagedAsync(int pageQty, int pageNum, bool includeInActive = false)
    {
        if (pageQty <= 0 || pageNum <= 0)
            return OperationResult<List<UserModel>>.Fail("Page quantity and Page number must be greater than 0.");
        Expression<Func<ApplicationUserModel, bool>>? predicate = includeInActive ?
            null : u => u.IsActive;
        return await GetAsync(predicate, pageQty, pageNum);
    }

    public async Task<OperationResult<List<UserModel>>> FilterByPagedAsync(
        Expression<Func<UserModel, bool>> predicate, int pageQty, int pageNum, bool includeInActive)
    {
        if (pageQty <= 0 || pageNum <= 0)
            return OperationResult<List<UserModel>>.Fail("Page quantity and Page number must be greater than 0.");

        OperationResult<Expression<Func<ApplicationUserModel, bool>>> finalPredRes =
            ConvertUserModExToAppUserModEx(predicate);
        if (!finalPredRes.Succeeded || finalPredRes.Data == null)
        {
            finalPredRes.AddError("Failed to filter users.");
            return OperationResult<List<UserModel>>.Fail(finalPredRes.Errors);
        }
        if (!includeInActive) finalPredRes = CombineAndAppUserModEx(finalPredRes.Data, u => u.IsActive);
        if (!finalPredRes.Succeeded || finalPredRes.Data == null)
        {
            finalPredRes.AddError("Failed to filter users.");
            return OperationResult<List<UserModel>>.Fail(finalPredRes.Errors);
        }

        return await GetAsync(finalPredRes.Data, pageQty, pageNum);
    }

    public async Task<OperationResult> CheckExistByAsync(Expression<Func<UserModel, bool>> predicate, bool includeInActive = false)
    {
        OperationResult<Expression<Func<ApplicationUserModel, bool>>> appUserPredRes = ConvertUserModExToAppUserModEx(predicate);
        if (!appUserPredRes.Succeeded || appUserPredRes.Data == null)
        {
            appUserPredRes.AddError("Failed to check user existance.");
            return OperationResult<List<UserModel>>.Fail(appUserPredRes.Errors);
        }

        Expression<Func<ApplicationUserModel, bool>> appUserPredicate = appUserPredRes.Data;
        bool exists;
        try
        {
            exists = includeInActive ? await _userManager.Users.AnyAsync(appUserPredicate)
               : await _userManager.Users.Where(u => u.IsActive).AnyAsync(appUserPredicate);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on checking existance of a user.");
            return OperationResult.Fail($"An error occurred while checking existance of a user.");
        }
        return exists ? OperationResult.Success() : OperationResult.Fail();
    }

    public async Task<OperationResult<UserModel>> GetByIdAsync(string id)
    {
        UserModel? user = null;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(id);
            if (appUser != null) user = _mapper.Map<ApplicationUserModel, UserModel>(appUser);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on getting user by Id.");
            return OperationResult<UserModel>.Fail($"An error occurred while retrieving the user by Id'.");
        }

        return user == null
            ? OperationResult<UserModel>.Fail($"User with id '{id}' is not found.")
            : OperationResult<UserModel>.Success(user);
    }

    public async Task<OperationResult<UserModel>> AddAsync(UserModel user, string password, string? currUserId)
    {
        OperationResult chkResult = await PrepareToAddUpdate(user, currUserId, true);
        if (!chkResult.Succeeded) return OperationResult<UserModel>.Fail(chkResult.Errors);

        IdentityResult result;
        try
        {
            ApplicationUserModel appUser = _mapper.Map<UserModel, ApplicationUserModel>(user);
            result = await _userManager.CreateAsync(appUser, password);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on creating a new user.");
            return OperationResult<UserModel>.Fail($"An error occurred while creating a new user.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { "Failed to add new user." };
            errors.AddRange(result.Errors.Select(er => $"Code: {er.Code}, Description: {er.Description}"));
            return OperationResult<UserModel>.Fail(errors);
        }

        OperationResult<List<UserModel>> fetchRes =
            await FilterByAsync(u => u.UserName == user.UserName && u.CreatedAt == user.CreatedAt);
        if (!fetchRes.Succeeded || fetchRes.Data == null || fetchRes.Data.Count == 0)
            return OperationResult<UserModel>.Success(null, "User added successfully, but failed to fetch it.");
        if (fetchRes.Data.Count > 1)
            return OperationResult<UserModel>.Success(fetchRes.Data[0],
                "User added successfully, but found more than user with same UserName and CreatedAt, one of them returned.");

        return OperationResult<UserModel>.Success(fetchRes.Data[0]);
    }

    public async Task<OperationResult<UserModel>> UpdateAsync(UserModel user, string currUserId)
    {
        if (user == null) return OperationResult<UserModel>.Fail("User can't be null.");

        OperationResult<UserModel> fetchingResult = await GetByIdAsync(user.Id);
        if (!fetchingResult.Succeeded || fetchingResult.Data == null)
        {
            fetchingResult.AddError($"Failed to fetch original user.");
            return fetchingResult;
        }
        if (!fetchingResult.Data.IsActive)
            return OperationResult<UserModel>.Fail($"Failed to update soft-deleted user, it must be restored first");

        OperationResult chkResult = await PrepareToAddUpdate(user, currUserId, false);
        if (!chkResult.Succeeded) return OperationResult<UserModel>.Fail(chkResult.Errors);

        IdentityResult result;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(user.Id);
            if (appUser == null) return OperationResult<UserModel>.Fail("Failed to fetch original User.");
            appUser = _mapper.Map(user, appUser);
            result = await _userManager.UpdateAsync(appUser);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on updating user.");
            return OperationResult<UserModel>.Fail($"An error occurred while updating user.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { "Failed to update user." };
            errors.AddRange(result.Errors.Select(er => $"Code: {er.Code}, Description: {er.Description}"));
            return OperationResult<UserModel>.Fail(errors);
        }

        OperationResult<UserModel> updUserRes = await GetByIdAsync(user.Id);
        return (updUserRes.Succeeded && updUserRes.Data != null)
            ? updUserRes
            : OperationResult<UserModel>.Success(null,
                "User has been updated successfully, but failed to fetch updated user.\r\n"
                + string.Join("\r\n", updUserRes.Errors));
    }

    public async Task<OperationResult<string>> SoftDeleteAsync(string userId, string currUserId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult<string>.Fail("Id can't be empty.");

        OperationResult chkResult = await CheckCurrUser(currUserId);
        if (!chkResult.Succeeded) return OperationResult<string>.Fail(chkResult.Errors);

        IdentityResult result;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult<string>.Fail("Failed to fetch User.");
            appUser.IsActive = false;
            result = await _userManager.UpdateAsync(appUser);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on soft-deleting user.");
            return OperationResult<string>.Fail($"An error occurred while soft-deleting user.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { "Failed to soft-delete user." };
            errors.AddRange(result.Errors.Select(er => $"Code: {er.Code}, Description: {er.Description}"));
            return OperationResult<string>.Fail(errors);
        }

        return OperationResult<string>.Success(userId);
    }

    public async Task<OperationResult<UserModel>> RestoreAsync(string userId, string currUserId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult<UserModel>.Fail("Id can't be empty.");

        OperationResult chkResult = await CheckCurrUser(currUserId);
        if (!chkResult.Succeeded) return OperationResult<UserModel>.Fail(chkResult.Errors);

        IdentityResult result;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult<UserModel>.Fail("Failed to fetch User.");
            appUser.IsActive = true;
            result = await _userManager.UpdateAsync(appUser);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on restoring user.");
            return OperationResult<UserModel>.Fail($"An error occurred while restoring user.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { "Failed to restore user." };
            errors.AddRange(result.Errors.Select(er => $"Code: {er.Code}, Description: {er.Description}"));
            return OperationResult<UserModel>.Fail(errors);
        }

        OperationResult<UserModel> updUserRes = await GetByIdAsync(userId);
        return (updUserRes.Succeeded && updUserRes.Data != null)
            ? updUserRes
            : OperationResult<UserModel>.Success(null,
                "User has been restored successfully, but failed to fetch it.\r\n"
                + string.Join("\r\n", updUserRes.Errors));
    }

    async Task<OperationResult<List<UserModel>>> GetAsync(Expression<Func<ApplicationUserModel, bool>>? predicate, int? pageQty, int? pageNum)
    {
        IQueryable<ApplicationUserModel> appUsersQuer = predicate == null ?
            _userManager.Users : _userManager.Users.Where(predicate);
        if (pageQty != null && pageNum != null) appUsersQuer = appUsersQuer
                .Skip(pageQty.Value * (pageNum.Value - 1)).Take(pageQty.Value);

        List<UserModel> users;
        try
        {
            List<ApplicationUserModel> appUsers = await appUsersQuer.ToListAsync();
            users = _mapper.Map<List<ApplicationUserModel>, List<UserModel>>(appUsers);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on retrieving entities of users.");
            return OperationResult<List<UserModel>>.Fail($"An error occurred while retrieving records of users.");
        }
        return OperationResult<List<UserModel>>.Success(users);
    }

    async Task<OperationResult> PrepareToAddUpdate(UserModel user, string? currUserId, bool isNewUser)
    {
        if (user == null) return OperationResult.Fail("User can't be null.");

        OperationResult chkResult = await CheckCurrUser(currUserId, isNewUser);
        if (!chkResult.Succeeded) return chkResult;

        chkResult = await SetCreatedProps(user, currUserId, isNewUser);
        if (!chkResult.Succeeded) return chkResult;

        SetUpdatedProps(user, currUserId, isNewUser);

        if (isNewUser) user.IsActive = true;

        chkResult = ValidateEntity(user);
        if (!chkResult.Succeeded) return chkResult;

        return OperationResult.Success();
    }

    #endregion

    #region Login management methods

    public async Task<OperationResult<UserModel>> CheckUserNameLoginAsync(string userName, string password)
    {
        return await CheckLoginAsync("UserName", userName, u => u.UserName == userName, password);
    }

    public async Task<OperationResult<UserModel>> CheckEmailLoginAsync(string email, string password)
    {
        return await CheckLoginAsync("Email", email, u => u.Email == email, password);
    }

    async Task<OperationResult<UserModel>> CheckLoginAsync(string loginParameterName, string loginParameter,
        Expression<Func<UserModel, bool>> parameterFilterPredicate, string password)
    {
        if (string.IsNullOrEmpty(loginParameterName)) return OperationResult<UserModel>.Fail("Missing parameter.");
        if (string.IsNullOrWhiteSpace(loginParameter)) return OperationResult<UserModel>.Fail($"{loginParameterName} can't be empty.");
        if (string.IsNullOrWhiteSpace(password)) return OperationResult<UserModel>.Fail("Password can't be empty.");

        OperationResult<List<UserModel>> usersRes = await FilterByAsync(parameterFilterPredicate);
        if (!usersRes.Succeeded || usersRes.Data == null)
        {
            List<string> errors = new() { $"Failed to get users by {loginParameterName}." };
            errors.AddRange(usersRes.Errors);
            return OperationResult<UserModel>.Fail(errors);
        }
        if (usersRes.Data.Count == 0) return OperationResult<UserModel>.Fail($"No user with {loginParameterName} '{loginParameter}'");

        ApplicationUserModel? appUser;
        foreach (UserModel user in usersRes.Data)
        {
            try
            {
                appUser = await _userManager.FindByIdAsync(user.Id);

                if (appUser != null && await _userManager.CheckPasswordAsync(appUser, password))
                    if (!appUser.IsActive) return OperationResult<UserModel>.Fail(
                        $"User with Id '{appUser.Id}' is soft-deleted, it must be restored first.");
                    else if (await _userManager.IsLockedOutAsync(appUser))
                        return OperationResult<UserModel>.Fail(
                            $"User with Id '{appUser.Id}' is locked, it must be unlocked first.");
                    else return OperationResult<UserModel>.Success(_mapper.Map<ApplicationUserModel, UserModel>(appUser));
            }
            catch (Exception ex)
            {
                LogError(LayerName, ex, $"Error on checking login for user.");
                return OperationResult<UserModel>.Fail($"An error occurred while checking login for user.");
            }
        }

        return OperationResult<UserModel>.Fail("Wrong Password");
    }

    public async Task<OperationResult> LockAsync(string userId, string currUserId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult.Fail("UserId can't be empty.");
        OperationResult chkResult = await CheckCurrUser(currUserId);
        if (!chkResult.Succeeded)
        {
            chkResult.AddError($"Failed to lock user with Id '{userId}'");
            return OperationResult.Fail(chkResult.Errors);
        }

        IdentityResult result;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult.Fail($"There is no user with Id '{userId}'");
            result = await _userManager.SetLockoutEndDateAsync(appUser, DateTimeOffset.MaxValue);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on locking user with Id '{userId}'.");
            return OperationResult<UserModel>.Fail($"An error occurred while locking user with Id '{userId}'.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { $"Failed to lock user with Id '{userId}'." };
            errors.AddRange(result.Errors.Select(e => $"Code: {e.Code}, Description: {e.Description}."));
            return OperationResult.Fail(errors);
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> UnLockAsync(string userId, string currUserId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult.Fail("UserId can't be empty.");
        OperationResult chkResult = await CheckCurrUser(currUserId);
        if (!chkResult.Succeeded)
        {
            chkResult.AddError($"Failed to unlock user with Id '{userId}'.");
            return OperationResult.Fail(chkResult.Errors);
        }

        IdentityResult result;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult.Fail($"There is no user with Id '{userId}'.");
            result = await _userManager.SetLockoutEndDateAsync(appUser, null);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on unlocking user with Id '{userId}'.");
            return OperationResult<UserModel>.Fail($"An error occurred while unlocking user with Id '{userId}'.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { $"Failed to unlock user with Id '{userId}'." };
            errors.AddRange(result.Errors.Select(e => $"Code: {e.Code}, Description: {e.Description}."));
            return OperationResult.Fail(errors);
        }

        return OperationResult.Success();
    }

    #endregion

    #region Password Management

    public async Task<OperationResult<UserModel>> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string currUserId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult<UserModel>.Fail("UserId can't be empty.");
        if (string.IsNullOrWhiteSpace(currentPassword)) return OperationResult<UserModel>.Fail("Current Password can't be empty.");
        if (string.IsNullOrWhiteSpace(newPassword)) return OperationResult<UserModel>.Fail("New Password can't be empty.");
        OperationResult chkResult = await CheckCurrUser(currUserId);
        if (!chkResult.Succeeded) return OperationResult<UserModel>.Fail(chkResult.Errors);

        ApplicationUserModel? appUser;
        IdentityResult result;
        try
        {
            appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult<UserModel>.Fail($"There is no user with Id '{userId}'");
            result = await _userManager.ChangePasswordAsync(appUser, currentPassword, newPassword);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on changing user password.");
            return OperationResult<UserModel>.Fail($"An error occurred while changing user password.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { "Failed to change password." };
            errors.AddRange(result.Errors.Select(e => $"Code: {e.Code}, Description: {e.Description}."));
            return OperationResult<UserModel>.Fail(errors);
        }

        appUser.UpdatedAt = DateTime.Now;
        appUser.UpdatedBy = currUserId;
        await _userManager.UpdateAsync(appUser);

        return OperationResult<UserModel>.Success(_mapper.Map<ApplicationUserModel, UserModel>(appUser));
    }

    public async Task<OperationResult<string>> GeneratePasswordResetTokenAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult<string>.Fail("UserId can't be empty.");
        string token;
        try
        {
            ApplicationUserModel? appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult<string>.Fail($"There is no user with Id '{userId}'");
            token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on generatint password reset token.");
            return OperationResult<string>.Fail($"An error occurred while generatint password reset token.");
        }

        return OperationResult<string>.Success(data: token);
    }

    public async Task<OperationResult<UserModel>> ResetPasswordAsync(string userId, string token, string newPassword, string currUserId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return OperationResult<UserModel>.Fail("UserId can't be empty.");
        if (string.IsNullOrWhiteSpace(token)) return OperationResult<UserModel>.Fail("Token can't be empty.");
        if (string.IsNullOrWhiteSpace(newPassword)) return OperationResult<UserModel>.Fail("NewPassword can't be empty.");
        OperationResult chkResult = await CheckCurrUser(currUserId);
        if (!chkResult.Succeeded) return OperationResult<UserModel>.Fail(chkResult.Errors);

        ApplicationUserModel? appUser;
        IdentityResult result;
        try
        {
            appUser = await _userManager.FindByIdAsync(userId);
            if (appUser == null) return OperationResult<UserModel>.Fail($"There is no user with Id '{userId}'");
            result = await _userManager.ResetPasswordAsync(appUser, token, newPassword);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, $"Error on reseting password.");
            return OperationResult<UserModel>.Fail($"An error occurred while reseting password.");
        }

        if (!result.Succeeded)
        {
            List<string> errors = new() { "Failed to reset password." };
            errors.AddRange(result.Errors.Select(e => $"Code: {e.Code}, Description: {e.Description}."));
            return OperationResult<UserModel>.Fail(errors);
        }

        appUser.UpdatedAt = DateTime.Now;
        appUser.UpdatedBy = currUserId;
        await _userManager.UpdateAsync(appUser);

        return OperationResult<UserModel>.Success(_mapper.Map<ApplicationUserModel, UserModel>(appUser));
    }

    #endregion

    #region Helper methods

    OperationResult<Expression<Func<ApplicationUserModel, bool>>> CombineAndAppUserModEx(
        Expression<Func<ApplicationUserModel, bool>> expr1, Expression<Func<ApplicationUserModel, bool>> expr2)
    {
        Expression<Func<ApplicationUserModel, bool>> finalExpr;
        try
        {
            finalExpr = _expressionCombiner.And(expr1, expr2);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, "Error on combining two ApplicationUserModel expressions.");
            return OperationResult<Expression<Func<ApplicationUserModel, bool>>>.Fail(
                $"An error occurred while combining two ApplicationUserModel expressions.");
        }
        return OperationResult<Expression<Func<ApplicationUserModel, bool>>>.Success(finalExpr);
    }

    OperationResult<Expression<Func<ApplicationUserModel, bool>>> ConvertUserModExToAppUserModEx(Expression<Func<UserModel, bool>> userPredicate)
    {
        Expression<Func<ApplicationUserModel, bool>> appUserPredicate;
        try
        {
            appUserPredicate = _expressionConverter.Convert(userPredicate);
        }
        catch (Exception ex)
        {
            LogError(LayerName, ex, "Error on converting UserModel expression to ApplicationUserModel expression.");
            return OperationResult<Expression<Func<ApplicationUserModel, bool>>>.Fail(
                $"An error occurred while converting UserModel expression to ApplicationUserModel expression.");
        }
        return OperationResult<Expression<Func<ApplicationUserModel, bool>>>.Success(appUserPredicate);
    }

    async Task<OperationResult> CheckCurrUser(string? currUserId, bool addingNewUser = false)
    {
        if (string.IsNullOrWhiteSpace(currUserId))
        {
            if (!addingNewUser) return OperationResult.Fail("Current user Id can't be empty");
            if ((await CheckExistByAsync(u => true, true)).Succeeded)
                return OperationResult.Fail("Current user Id is required since there this is not first User");
            return OperationResult.Success();
        }

        OperationResult userExistRes = await CheckExistByAsync(u => u.Id == currUserId);
        if (!userExistRes.Succeeded)
        {
            userExistRes.Errors.Add($"There is no user with id '{currUserId}'.");
            return OperationResult.Fail(userExistRes.Errors);
        }

        return OperationResult.Success();
    }

    async Task<OperationResult> SetCreatedProps(UserModel user, string? currUserId, bool isUserNew)
    {
        if (isUserNew)
        {
            user.CreatedBy = currUserId;
            user.CreatedAt = DateTime.Now;
            return OperationResult.Success();
        }

        OperationResult<UserModel> fetchResult = await GetByIdAsync(user.Id);
        if (!fetchResult.Succeeded || fetchResult.Data == null)
        {
            fetchResult.AddError($"Failed to fetch original user.");
            return OperationResult.Fail(fetchResult.Errors);
        }

        user.CreatedBy = fetchResult.Data.CreatedBy;
        user.CreatedAt = fetchResult.Data.CreatedAt;

        return OperationResult.Success();
    }

    void SetUpdatedProps(UserModel user, string currUserId, bool isUserNew)
    {
        if (isUserNew)
        {
            user.UpdatedBy = null;
            user.UpdatedAt = null;
            return;
        }

        user.UpdatedBy = currUserId;
        user.UpdatedAt = DateTime.Now;
    }

    OperationResult ValidateEntity(UserModel user)
    {
        if (user == null) return OperationResult.Fail("User can't be null.");
        return _validator.ValidateEntity(user);
    }

    void LogError(string layer, Exception ex, string customMsg)
    {
        _logger.LogError(layer, ex, customMsg);
    }

    #endregion

    #region Map mthods

    public OperationResult<UserModel> MapModelDTO(UserDTO dto)
    {
        string failMsg = $"Failed to map UserDTO to UserModel.";

        if (dto == null) return OperationResult<UserModel>.Fail(failMsg, "dto can't be null.");
        UserModel? model;

        try
        {
            model = _mapper.Map<UserDTO, UserModel>(dto);
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, failMsg);
            return OperationResult<UserModel>.Fail(failMsg);
        }

        return OperationResult<UserModel>.Success(model);
    }

    public OperationResult<UserDTO> MapModelDTO(UserModel model)
    {
        string failMsg = $"Failed to map UserModel to UserDTO.";

        if (model == null) return OperationResult<UserDTO>.Fail(failMsg, "Model can't be null.");
        UserDTO? dto;

        try
        {
            dto = _mapper.Map<UserModel, UserDTO>(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, failMsg);
            return OperationResult<UserDTO>.Fail(failMsg);
        }

        return OperationResult<UserDTO>.Success(dto);
    }

    public OperationResult<List<UserModel>> MapModelDTO(List<UserDTO> dtos)
    {
        string failMsg = $"Failed to map List<UserDTO> to List<UserModel>.";

        if (dtos == null) return OperationResult<List<UserModel>>.Fail(failMsg, "dtos can't be null.");
        if (dtos.Count == 0) return OperationResult<List<UserModel>>.Success(new());
        List<UserModel>? models;

        try
        {
            models = _mapper.Map<List<UserDTO>, List<UserModel>>(dtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, failMsg);
            return OperationResult<List<UserModel>>.Fail(failMsg);
        }

        return OperationResult<List<UserModel>>.Success(models);
    }

    public OperationResult<List<UserDTO>> MapModelDTO(List<UserModel> models)
    {
        string failMsg = $"Failed to map List<UserModel> to List<UserDTO>.";

        if (models == null) return OperationResult<List<UserDTO>>.Fail(failMsg, "Models can't be null.");
        if (models.Count == 0) return OperationResult<List<UserDTO>>.Success(new());
        List<UserDTO> dtos;

        try
        {
            dtos = _mapper.Map<List<UserModel>, List<UserDTO>>(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, failMsg);
            return OperationResult<List<UserDTO>>.Fail(failMsg);
        }

        return OperationResult<List<UserDTO>>.Success(dtos);
    }

    public OperationResult<UserModel> MapModelDTO(OperationResult<UserDTO> dtoRes)
    {
        OperationResult<UserModel> modelRes = dtoRes.Succeeded
            ? OperationResult<UserModel>.Success(null) : OperationResult<UserModel>.Fail(dtoRes.Errors);
        modelRes.Message = dtoRes.Message;
        modelRes.Errors = dtoRes.Errors;
        if (dtoRes.Data == null) return modelRes;

        OperationResult<UserModel> dataDtoToModRes = MapModelDTO(dtoRes.Data);
        if (dataDtoToModRes.Succeeded && dataDtoToModRes.Data != null)
        {
            modelRes.Data = dataDtoToModRes.Data;
            return modelRes;
        }

        modelRes.Errors.AddRange(dataDtoToModRes.Errors);
        modelRes.AddError($"Failed to map OperationResult<UserDTO> to OperationResult<UserModel>.");

        return modelRes;
    }

    public OperationResult<UserDTO> MapModelDTO(OperationResult<UserModel> modelRes)
    {
        OperationResult<UserDTO> dtoRes = modelRes.Succeeded
            ? OperationResult<UserDTO>.Success(null) : OperationResult<UserDTO>.Fail(modelRes.Errors);
        dtoRes.Message = modelRes.Message;
        dtoRes.Errors = modelRes.Errors;
        if (modelRes.Data == null) return dtoRes;

        OperationResult<UserDTO> dataModToDtoRes = MapModelDTO(modelRes.Data);
        if (dataModToDtoRes.Succeeded && dataModToDtoRes.Data != null)
        {
            dtoRes.Data = dataModToDtoRes.Data;
            return dtoRes;
        }

        dtoRes.Errors.AddRange(dataModToDtoRes.Errors);
        dtoRes.AddError($"Failed to map OperationResult<UserModel> to OperationResult<UserDTO>.");

        return dtoRes;
    }

    public OperationResult<List<UserModel>> MapModelDTO(OperationResult<List<UserDTO>> dtosRes)
    {
        OperationResult<List<UserModel>> modelsRes = dtosRes.Succeeded
            ? OperationResult<List<UserModel>>.Success(null) : OperationResult<List<UserModel>>.Fail(dtosRes.Errors);
        modelsRes.Message = dtosRes.Message;
        modelsRes.Errors = dtosRes.Errors;
        if (dtosRes.Data == null) return modelsRes;
        if (dtosRes.Data.Count == 0)
        {
            modelsRes.Data = new();
            return modelsRes;
        }

        OperationResult<List<UserModel>> dataDtoToModRes = MapModelDTO(dtosRes.Data);
        if (dataDtoToModRes.Succeeded && dataDtoToModRes.Data != null)
        {
            modelsRes.Data = dataDtoToModRes.Data;
            return modelsRes;
        }

        modelsRes.Errors.AddRange(dataDtoToModRes.Errors);
        modelsRes.AddError($"Failed to map OperationResult<List<UserDTO>> to OperationResult<List<UserModel>>.");

        return modelsRes;
    }

    public OperationResult<List<UserDTO>> MapModelDTO(OperationResult<List<UserModel>> modelsRes)
    {
        OperationResult<List<UserDTO>> dtosRes = modelsRes.Succeeded
            ? OperationResult<List<UserDTO>>.Success(null) : OperationResult<List<UserDTO>>.Fail(modelsRes.Errors);
        dtosRes.Message = modelsRes.Message;
        dtosRes.Errors = modelsRes.Errors;
        if (modelsRes.Data == null) return dtosRes;
        if (modelsRes.Data.Count == 0)
        {
            dtosRes.Data = new();
            return dtosRes;
        }

        OperationResult<List<UserDTO>> dataModToDtoRes = MapModelDTO(modelsRes.Data);
        if (dataModToDtoRes.Succeeded && dataModToDtoRes.Data != null)
        {
            dtosRes.Data = dataModToDtoRes.Data;
            return dtosRes;
        }

        dtosRes.Errors.AddRange(dataModToDtoRes.Errors);
        dtosRes.AddError($"Failed to map OperationResult<List<UserModel>> to OperationResult<List<UserDTO>>.");

        return dtosRes;
    }

    #endregion

}