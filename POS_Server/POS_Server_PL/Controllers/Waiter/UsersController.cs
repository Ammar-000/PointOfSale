using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.DTOs;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Models.RequestsModels.DTOsRequstsModels;
using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Controllers.Waiter;

[Authorize(Roles = "Admin, Waiter")]
[Route("api/Waiter/[controller]")]
[ApiController]
public class UsersController : BaseApiController
{
    readonly IUserService _userService;

    public UsersController(IUserService userService, ICustomLogger logger) : base(logger)
    {
        _userService = userService;
    }

    [HttpGet("GetMyInfo")]
    public async Task<ActionResult<OperationResult<UserDTO>>> GetMyInfoAsync()
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<UserDTO>.Fail(currUserIdRes.Errors));

        OperationResult<UserDTO> result = _userService.MapModelDTO(await _userService.GetByIdAsync(currUserIdRes.Data));
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("ChangePassword")]
    public async Task<ActionResult<OperationResult<UserDTO>>> ChangePasswordAsync(
        [Required][FromBody] ChangePasswordDTOModel changePasswordModel)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<UserDTO>.Fail(currUserIdRes.Errors));

        OperationResult<UserDTO> result = _userService.MapModelDTO(await _userService.ChangePasswordAsync(currUserIdRes.Data,
            changePasswordModel.CurrentPassword, changePasswordModel.NewPassword, currUserIdRes.Data));
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("ResetPassword")]
    public async Task<ActionResult<OperationResult<UserDTO>>> ResetPasswordAsync(
        [Required][FromBody] ResetPasswordDTOModel resetPasswordModel)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<UserDTO>.Fail(currUserIdRes.Errors));

        OperationResult<UserDTO> result = _userService.MapModelDTO(await _userService.ResetPasswordAsync(currUserIdRes.Data,
            resetPasswordModel.ResetPasswordToken, resetPasswordModel.NewPassword, currUserIdRes.Data));
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

}
