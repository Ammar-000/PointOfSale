using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.DTOs;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.BaseInterfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace POS_Server_PL.Controllers.Waiter.BaseControllers;

public class SoftDeletableController<TModel, TDTO, TService> : BaseController<TModel, TDTO, TService>
    where TModel : BaseSoftDeletableModel where TDTO : BaseDTO where TService : ISoftDeletableService<TModel>
{
    readonly TService _softService;

    public SoftDeletableController(TService softService, ICustomLogger logger) : base(softService, logger)
    {
        _softService = softService;
    }

    [HttpGet("Count")]
    public virtual async Task<ActionResult<OperationResult<int>>> CountAsync()
    {
        OperationResult<int> result = await _softService.CountAsync();
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("GetAll")]
    public virtual async Task<ActionResult<OperationResult<List<TDTO>>>> GetAllAsync()
    {
        OperationResult<List<TDTO>> result = _service.MapModelDTO<TDTO>(await _softService.GetAllAsync());
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

    [HttpGet("GetAllPaged")]
    public virtual async Task<ActionResult<OperationResult<List<TDTO>>>> GetAllPagedAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum)
    {
        OperationResult<List<TDTO>> result =
            _service.MapModelDTO<TDTO>(await _softService.GetAllPagedAsync(pageQty, pageNum));
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
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
    public virtual async Task<ActionResult<OperationResult<TDTO>>> RestoreAsync(
        [Required][FromBody][Range(1, int.MaxValue)] int id)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<TModel>.Fail(currUserIdRes.Errors));

        OperationResult<TDTO> result = _service.MapModelDTO<TDTO>(await _softService.RestoreAsync(id, currUserIdRes.Data));
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

    protected async Task<ActionResult<OperationResult<List<TDTO>>>> FilterByAsync(
        Expression<Func<TModel, bool>> predicate)
    {
        OperationResult<List<TDTO>> result = _service.MapModelDTO<TDTO>(await _softService.FilterByAsync(predicate));
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

    protected async Task<ActionResult<OperationResult<List<TDTO>>>> FilterByPagedAsync(
        Expression<Func<TModel, bool>> predicate, int pageQty, int pageNum)
    {
        OperationResult<List<TDTO>> result =
            _service.MapModelDTO<TDTO>(await _softService.FilterByPagedAsync(predicate, pageQty, pageNum));

        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

}
