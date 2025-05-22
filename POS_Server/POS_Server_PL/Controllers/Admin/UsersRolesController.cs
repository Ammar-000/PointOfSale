using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Models.RequestsModels.ModelsRequstsModels;
using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Controllers.Admin;

[Route("api/Admin/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UsersRolesController : BaseApiController
{
    readonly IUsersRolesService _usersRolesService;

    public UsersRolesController(IUsersRolesService usersRolesService, ICustomLogger logger) : base(logger)
    {
        _usersRolesService = usersRolesService;
    }

    [HttpPost("GetUserRolesNames")]
    public async Task<ActionResult<OperationResult<List<string>>>> GetUserRolesNamesAsync([Required][FromBody] string userId)
    {
        OperationResult<List<string>> result = await _usersRolesService.GetUserRolesNamesAsync(userId);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("GetMyRolesNames")]
    public async Task<ActionResult<OperationResult<List<string>>>> GetMyRolesAsync()
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<List<string>>.Fail(currUserIdRes.Errors));

        OperationResult<List<string>> result = await _usersRolesService.GetUserRolesNamesAsync(currUserIdRes.Data);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("GetUsersInRole")]
    public async Task<ActionResult<OperationResult<List<UserModel>>>> GetUsersInRoleAsync([Required][FromBody] string roleId)
    {
        OperationResult<List<UserModel>> result = await _usersRolesService.GetUsersInRoleAsync(roleId);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("IsInRole")]
    public async Task<ActionResult<OperationResult>> IsInRoleAsync(
        [Required][FromBody] ModifyUserRolesModel modifyUserRolesModel)
    {
        OperationResult result = await _usersRolesService.IsInRoleAsync(modifyUserRolesModel.UserId, modifyUserRolesModel.RoleId);
        return Ok(result);
    }

    [HttpPost("AddToRole")]
    public async Task<ActionResult<OperationResult>> AddToRoleAsync(
        [Required][FromBody] ModifyUserRolesModel modifyUserRolesModel)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult.Fail(currUserIdRes.Errors));

        OperationResult result = await _usersRolesService.AddToRoleAsync(
            modifyUserRolesModel.UserId, modifyUserRolesModel.RoleId, currUserIdRes.Data);
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("RemoveFromRole")]
    public async Task<ActionResult<OperationResult>> RemoveFromRoleAsync(
        [Required][FromBody] ModifyUserRolesModel modifyUserRolesModel)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult.Fail(currUserIdRes.Errors));

        OperationResult result = await _usersRolesService.RemoveFromRoleAsync(
            modifyUserRolesModel.UserId, modifyUserRolesModel.RoleId, currUserIdRes.Data);
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }
}
