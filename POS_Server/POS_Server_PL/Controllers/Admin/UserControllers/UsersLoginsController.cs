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
public class UsersLoginsController : BaseApiController
{
    readonly IUserService _userService;

    public UsersLoginsController(IUserService userService, ICustomLogger logger) : base(logger)
    {
        _userService = userService;
    }

    [HttpPost("CheckUserNameLogin")]
    public async Task<ActionResult<OperationResult<UserModel>>> CheckUserNameLoginAsync(
        [Required][FromBody] CheckLoginModel checkLoginModel)
    {
        OperationResult<UserModel> result =
            await _userService.CheckUserNameLoginAsync(checkLoginModel.UserNameOrEmail, checkLoginModel.Password);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("CheckEmailLogin")]
    public async Task<ActionResult<OperationResult<UserModel>>> CheckEmailLoginAsync(
        [Required][FromBody] CheckLoginModel checkLoginModel)
    {
        OperationResult<UserModel> result =
            await _userService.CheckEmailLoginAsync(checkLoginModel.UserNameOrEmail, checkLoginModel.Password);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("Lock")]
    public async Task<ActionResult<OperationResult>> LockAsync([Required][FromBody] string userId)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult.Fail(currUserIdRes.Errors));

        OperationResult result = await _userService.LockAsync(userId, currUserIdRes.Data);
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("UnLock")]
    public async Task<ActionResult<OperationResult>> UnLockAsync([Required][FromBody] string userId)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult.Fail(currUserIdRes.Errors));

        OperationResult result = await _userService.UnLockAsync(userId, currUserIdRes.Data);
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }
}
