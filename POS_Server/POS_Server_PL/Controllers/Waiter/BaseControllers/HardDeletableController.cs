using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.DTOs;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.BaseInterfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace POS_Server_PL.Controllers.Waiter.BaseControllers;

public class HardDeletableController<TModel, TDTO, TService> : BaseController<TModel, TDTO, TService>
    where TModel : BaseModel where TDTO : BaseDTO where TService : IHardDeletableService<TModel>
{
    readonly TService _hardService;

    public HardDeletableController(TService hardService, ICustomLogger logger) : base(hardService, logger)
    {
        _hardService = hardService;
    }

    [HttpGet("Count")]
    public virtual async Task<ActionResult<OperationResult<int>>> CountAsync()
    {
        OperationResult<int> result = await _hardService.CountAsync();
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("GetAll")]
    public virtual async Task<ActionResult<OperationResult<List<TDTO>>>> GetAllAsync()
    {
        OperationResult<List<TDTO>> result = _service.MapModelDTO<TDTO>(await _hardService.GetAllAsync());
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

    [HttpGet("GetAllPaged")]
    public virtual async Task<ActionResult<OperationResult<List<TDTO>>>> GetAllPagedAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum)
    {
        OperationResult<List<TDTO>> result = _service.MapModelDTO<TDTO>(await _hardService.GetAllPagedAsync(pageQty, pageNum));
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

    [HttpPost("HardDelete")]
    public virtual async Task<ActionResult<OperationResult<int>>> HardDeleteAsync(
        [Required][FromBody][Range(1, int.MaxValue)] int id)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<int>.Fail(currUserIdRes.Errors));

        OperationResult<int> result = await _hardService.HardDeleteAsync(id, currUserIdRes.Data);
        return result.Succeeded && result.Data > 0 ? Ok(result) : HandleInternalFailure(result);
    }

    protected async Task<ActionResult<OperationResult<List<TDTO>>>> FilterByAsync(
        Expression<Func<TModel, bool>> predicate)
    {
        OperationResult<List<TDTO>> result = _service.MapModelDTO<TDTO>(await _hardService.FilterByAsync(predicate));
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

    protected async Task<ActionResult<OperationResult<List<TDTO>>>> FilterByPagedAsync(
        Expression<Func<TModel, bool>> predicate, int pageQty, int pageNum)
    {
        OperationResult<List<TDTO>> result =
            _service.MapModelDTO<TDTO>(await _hardService.FilterByPagedAsync(predicate, pageQty, pageNum));

        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

}
