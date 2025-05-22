using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.BaseInterfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace POS_Server_PL.Controllers.Admin.BaseControllers;

public class SoftDeletableController<TModel, TService> : BaseController<TModel, TService>
    where TModel : BaseSoftDeletableModel where TService : ISoftDeletableService<TModel>
{
    readonly TService _softService;

    public SoftDeletableController(TService softService, ICustomLogger logger) : base(softService, logger)
    {
        _softService = softService;
    }

    [HttpGet("Count")]
    public virtual async Task<ActionResult<OperationResult<int>>> CountAsync([FromQuery] bool includeInActive = false)
    {
        OperationResult<int> result = await _softService.CountAsync(includeInActive);
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("GetAll")]
    public virtual async Task<ActionResult<OperationResult<List<TModel>>>> GetAllAsync([FromQuery] bool includeInActive = false)
    {
        OperationResult<List<TModel>> result = await _softService.GetAllAsync(includeInActive);
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        foreach (TModel model in result.Data) _service.RemoveNavigationProps(model);
        return Ok(result);
    }

    [HttpGet("GetAllPaged")]
    public virtual async Task<ActionResult<OperationResult<List<TModel>>>> GetAllPagedAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum,
        [FromQuery] bool includeInActive = false)
    {
        OperationResult<List<TModel>> result = await _softService.GetAllPagedAsync(pageQty, pageNum, includeInActive);
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        foreach (TModel model in result.Data) _service.RemoveNavigationProps(model);
        return Ok(result);
    }

    [HttpPost("SoftDelete")]
    public virtual async Task<ActionResult<OperationResult<int>>> SoftDeleteAsync(
        [Required][FromBody][Range(1, int.MaxValue)] int id)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<int>.Fail(currUserIdRes.Errors));

        OperationResult<int> result = await _softService.SoftDeleteAsync(id, currUserIdRes.Data);
        return result.Succeeded && result.Data > 0 ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("Restore")]
    public virtual async Task<ActionResult<OperationResult<TModel>>> RestoreAsync(
        [Required][FromBody][Range(1, int.MaxValue)] int id)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<TModel>.Fail(currUserIdRes.Errors));

        OperationResult<TModel> result = await _softService.RestoreAsync(id, currUserIdRes.Data);
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        _service.RemoveNavigationProps(result.Data);
        return Ok(result);
    }

    protected async Task<ActionResult<OperationResult<List<TModel>>>> FilterByAsync(
    Expression<Func<TModel, bool>> predicate, bool includeInActive = false)
    {
        OperationResult<List<TModel>> result = await _softService.FilterByAsync(predicate, includeInActive);
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        foreach (TModel model in result.Data) _service.RemoveNavigationProps(model);
        return Ok(result);
    }

    protected async Task<ActionResult<OperationResult<List<TModel>>>> FilterByPagedAsync(
    Expression<Func<TModel, bool>> predicate, int pageQty, int pageNum, bool includeInActive = false)
    {
        OperationResult<List<TModel>> result =
            await _softService.FilterByPagedAsync(predicate, pageQty, pageNum, includeInActive);

        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        foreach (TModel model in result.Data) _service.RemoveNavigationProps(model);
        return Ok(result);
    }

}
