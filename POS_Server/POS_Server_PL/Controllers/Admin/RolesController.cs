using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.FiltersModels;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace POS_Server_PL.Controllers.Admin;

[Route("api/Admin/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class RolesController : BaseApiController
{
    IRoleService _roleService;

    public RolesController(ICustomLogger logger, IRoleService roleService) : base(logger)
    {
        _roleService = roleService;
    }

    [HttpGet("Count")]
    public async Task<ActionResult<OperationResult<int>>> CountAsync()
    {
        OperationResult<int> result = await _roleService.CountAsync();
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("GetAll")]
    public async Task<ActionResult<OperationResult<List<RoleModel>>>> GetAllAsync()
    {
        OperationResult<List<RoleModel>> result = await _roleService.GetAllAsync();
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("FilterBy")]
    public async Task<ActionResult<OperationResult<List<RoleModel>>>> FilterByAsync(
        [Required][FromBody] RoleFilterModel roleFilter)
    {
        Expression<Func<RoleModel, bool>> predicate = r =>
            (roleFilter.Name == null || r.Name == roleFilter.Name) &&
            (roleFilter.Description == null || r.Description == roleFilter.Description) &&
            (roleFilter.CreatedBy == null || r.CreatedBy == roleFilter.CreatedBy) &&
            (roleFilter.UpdatedBy == null || r.UpdatedBy == roleFilter.UpdatedBy) &&
            (roleFilter.CreatedAt == null || r.CreatedAt == roleFilter.CreatedAt) &&
            (roleFilter.UpdatedAt == null || r.UpdatedAt == roleFilter.UpdatedAt);

        OperationResult<List<RoleModel>> result = await _roleService.FilterByAsync(predicate);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("GetAllPaged")]
    public async Task<ActionResult<OperationResult<List<RoleModel>>>> GetAllPagedAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum)
    {
        OperationResult<List<RoleModel>> result = await _roleService.GetAllPagedAsync(pageQty, pageNum);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("FilterByPaged")]
    public async Task<ActionResult<OperationResult<List<RoleModel>>>> FilterByPagedAsync(
        [Required][FromBody] RoleFilterModel roleFilter,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum)
    {
        Expression<Func<RoleModel, bool>> predicate = r =>
            (roleFilter.Name == null || r.Name == roleFilter.Name) &&
            (roleFilter.Description == null || r.Description == roleFilter.Description) &&
            (roleFilter.CreatedBy == null || r.CreatedBy == roleFilter.CreatedBy) &&
            (roleFilter.UpdatedBy == null || r.UpdatedBy == roleFilter.UpdatedBy) &&
            (roleFilter.CreatedAt == null || r.CreatedAt == roleFilter.CreatedAt) &&
            (roleFilter.UpdatedAt == null || r.UpdatedAt == roleFilter.UpdatedAt);

        OperationResult<List<RoleModel>> result = await _roleService.FilterByPagedAsync(predicate, pageQty, pageNum);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("CheckExistBy")]
    public async Task<ActionResult<OperationResult>> CheckExistByAsync(
        [Required][FromBody] RoleFilterModel roleFilter)
    {
        Expression<Func<RoleModel, bool>> predicate = r =>
            (roleFilter.Name == null || r.Name == roleFilter.Name) &&
            (roleFilter.Description == null || r.Description == roleFilter.Description) &&
            (roleFilter.CreatedBy == null || r.CreatedBy == roleFilter.CreatedBy) &&
            (roleFilter.UpdatedBy == null || r.UpdatedBy == roleFilter.UpdatedBy) &&
            (roleFilter.CreatedAt == null || r.CreatedAt == roleFilter.CreatedAt) &&
            (roleFilter.UpdatedAt == null || r.UpdatedAt == roleFilter.UpdatedAt);

        OperationResult result = await _roleService.CheckExistByAsync(predicate);
        return Ok(result);
    }

    [HttpPost("GetById")]
    public async Task<ActionResult<OperationResult<RoleModel>>> GetByIdAsync([Required][FromBody] string id)
    {
        OperationResult<RoleModel> result = await _roleService.GetByIdAsync(id);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("Add")]
    public async Task<ActionResult<OperationResult<RoleModel>>> AddAsync([Required][FromBody] RoleModel role)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<RoleModel>.Fail(currUserIdRes.Errors));

        OperationResult<RoleModel> result = await _roleService.AddAsync(role, currUserIdRes.Data);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPut("Update")]
    public async Task<ActionResult<OperationResult<RoleModel>>> UpdateAsync([Required][FromBody] RoleModel role)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<RoleModel>.Fail(currUserIdRes.Errors));

        OperationResult<RoleModel> result = await _roleService.UpdateAsync(role, currUserIdRes.Data);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("HardDelete")]
    public async Task<ActionResult<OperationResult<string>>> HardDeleteAsync([Required][FromBody] string roleId)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<string>.Fail(currUserIdRes.Errors));

        OperationResult<string> result = await _roleService.HardDeleteAsync(roleId, currUserIdRes.Data);
        return result.Succeeded && result.Data != null ? Ok(result) : HandleInternalFailure(result);
    }

}
