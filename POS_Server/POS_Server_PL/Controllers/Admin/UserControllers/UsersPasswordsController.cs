using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.UserRequestsModels;
using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Controllers.Admin.UserControllers;

[Route("api/Admin/Users/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UsersPasswordsController : BaseApiController
{
    readonly IUserService _userService;

    public UsersPasswordsController(IUserService userService, ICustomLogger logger) : base(logger)
    {
        _userService = userService;
    }

    [HttpPost("ChangePassword")]
    public async Task<ActionResult<OperationResult<UserModel>>> ChangePasswordAsync(
        [Required][FromBody] ChangePasswordModel changePasswordModel)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<UserModel>.Fail(currUserIdRes.Errors));

        OperationResult<UserModel> result = await _userService.ChangePasswordAsync(changePasswordModel.UserId,
            changePasswordModel.CurrentPassword, changePasswordModel.NewPassword, currUserIdRes.Data);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("GeneratePasswordResetToken")]
    public async Task<ActionResult<OperationResult<string>>> GeneratePasswordResetTokenAsync(
        [Required][FromBody] string userId)
    {
        OperationResult<string> result = await _userService.GeneratePasswordResetTokenAsync(userId);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("ResetPassword")]
    public async Task<ActionResult<OperationResult<UserModel>>> ResetPasswordAsync(
        [Required][FromBody] ResetPasswordModel resetPasswordModel)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<UserModel>.Fail(currUserIdRes.Errors));

        OperationResult<UserModel> result = await _userService.ResetPasswordAsync(resetPasswordModel.UserId,
            resetPasswordModel.ResetPasswordToken, resetPasswordModel.NewPassword, currUserIdRes.Data);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

}
