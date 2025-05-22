using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.UserRequestsModels;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace POS_Server_PL.Controllers.Admin.UserControllers;

[Route("api/Admin/Users/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UsersBasicsController : BaseApiController
{
    readonly IUserService _userService;

    public UsersBasicsController(IUserService userService, ICustomLogger logger) : base(logger)
    {
        _userService = userService;
    }

    [HttpGet("Count")]
    public async Task<ActionResult<OperationResult<int>>> CountAsync([FromQuery] bool includeInActive = false)
    {
        OperationResult<int> result = await _userService.CountAsync(includeInActive);
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("GetAll")]
    public async Task<ActionResult<OperationResult<List<UserModel>>>> GetAllAsync([FromQuery] bool includeInActive = false)
    {
        OperationResult<List<UserModel>> result = await _userService.GetAllAsync(includeInActive);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("FilterBy")]
    public async Task<ActionResult<OperationResult<List<UserModel>>>> FilterByAsync(
        [Required][FromBody] UserFilterModel userFilter, [FromQuery] bool includeInActive = false)
    {
        Expression<Func<UserModel, bool>> predicate = u =>
            (userFilter.UserName == null || u.UserName == userFilter.UserName) &&
            (userFilter.FirstName == null || u.FirstName == userFilter.FirstName) &&
            (userFilter.LastName == null || u.LastName == userFilter.LastName) &&
            (userFilter.Email == null || u.Email == userFilter.Email) &&
            (userFilter.CreatedAt == null || u.CreatedAt == userFilter.CreatedAt) &&
            (userFilter.CreatedBy == null || u.CreatedBy == userFilter.CreatedBy) &&
            (userFilter.UpdatedAt == null || u.UpdatedAt == userFilter.UpdatedAt) &&
            (userFilter.UpdatedBy == null || u.UpdatedBy == userFilter.UpdatedBy);

        OperationResult<List<UserModel>> result = await _userService.FilterByAsync(predicate, includeInActive);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("GetAllPaged")]
    public async Task<ActionResult<OperationResult<List<UserModel>>>> GetAllPagedAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum,
        [FromQuery] bool includeInActive = false)
    {
        OperationResult<List<UserModel>> result = await _userService.GetAllPagedAsync(pageQty, pageNum, includeInActive);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("FilterByPaged")]
    public async Task<ActionResult<OperationResult<List<UserModel>>>> FilterByPagedAsync(
        [Required][FromBody] UserFilterModel userFilter,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum,
        [FromQuery] bool includeInActive = false)
    {
        Expression<Func<UserModel, bool>> predicate = u =>
            (userFilter.UserName == null || u.UserName == userFilter.UserName) &&
            (userFilter.FirstName == null || u.FirstName == userFilter.FirstName) &&
            (userFilter.LastName == null || u.LastName == userFilter.LastName) &&
            (userFilter.Email == null || u.Email == userFilter.Email) &&
            (userFilter.CreatedAt == null || u.CreatedAt == userFilter.CreatedAt) &&
            (userFilter.CreatedBy == null || u.CreatedBy == userFilter.CreatedBy) &&
            (userFilter.UpdatedAt == null || u.UpdatedAt == userFilter.UpdatedAt) &&
            (userFilter.UpdatedBy == null || u.UpdatedBy == userFilter.UpdatedBy);

        OperationResult<List<UserModel>> result =
            await _userService.FilterByPagedAsync(predicate, pageQty, pageNum, includeInActive);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("CheckExistBy")]
    public async Task<ActionResult<OperationResult>> CheckExistByAsync(
        [Required][FromBody] UserFilterModel userFilter, [FromQuery] bool includeInActive = false)
    {
        Expression<Func<UserModel, bool>> predicate = u =>
            (userFilter.UserName == null || u.UserName == userFilter.UserName) &&
            (userFilter.FirstName == null || u.FirstName == userFilter.FirstName) &&
            (userFilter.LastName == null || u.LastName == userFilter.LastName) &&
            (userFilter.Email == null || u.Email == userFilter.Email) &&
            (userFilter.CreatedAt == null || u.CreatedAt == userFilter.CreatedAt) &&
            (userFilter.CreatedBy == null || u.CreatedBy == userFilter.CreatedBy) &&
            (userFilter.UpdatedAt == null || u.UpdatedAt == userFilter.UpdatedAt) &&
            (userFilter.UpdatedBy == null || u.UpdatedBy == userFilter.UpdatedBy);

        OperationResult result = await _userService.CheckExistByAsync(predicate, includeInActive);
        return Ok(result);
    }

    [HttpPost("GetById")]
    public async Task<ActionResult<OperationResult<UserModel>>> GetByIdAsync([Required][FromBody] string id)
    {
        OperationResult<UserModel> result = await _userService.GetByIdAsync(id);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("GetMyInfo")]
    public async Task<ActionResult<OperationResult<UserModel>>> GetMyInfoAsync()
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<UserModel>.Fail(currUserIdRes.Errors));

        OperationResult<UserModel> result = await _userService.GetByIdAsync(currUserIdRes.Data);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("Add")]
    public async Task<ActionResult<OperationResult<UserModel>>> AddAsync(
        [Required][FromBody] AddUserModel addUserModel)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<UserModel>.Fail(currUserIdRes.Errors));

        OperationResult<UserModel> result =
            await _userService.AddAsync(addUserModel.User, addUserModel.Password, currUserIdRes.Data);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPut("Update")]
    public async Task<ActionResult<OperationResult<UserModel>>> UpdateAsync([Required][FromBody] UserModel user)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<UserModel>.Fail(currUserIdRes.Errors));

        OperationResult<UserModel> result = await _userService.UpdateAsync(user, currUserIdRes.Data);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("SoftDelete")]
    public async Task<ActionResult<OperationResult<string>>> SoftDeleteAsync([Required][FromBody] string userId)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<string>.Fail(currUserIdRes.Errors));

        OperationResult<string> result = await _userService.SoftDeleteAsync(userId, currUserIdRes.Data);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("Restore")]
    public async Task<ActionResult<OperationResult<UserModel>>> RestoreAsync([Required][FromBody] string userId)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<UserModel>.Fail(currUserIdRes.Errors));

        OperationResult<UserModel> result = await _userService.RestoreAsync(userId, currUserIdRes.Data);
        return result.Succeeded && result.Data != null
            ? Ok(result) : HandleInternalFailure(result);
    }

}
