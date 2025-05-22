using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.DTOs;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.BaseInterfaces;
using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Controllers.Waiter.BaseControllers;

[Authorize(Roles = "Admin, Waiter")]
[Route("api/Waiter/[controller]")]
[ApiController]
public abstract class BaseController<TModel, TDTO, TService> : BaseApiController
    where TModel : BaseModel where TDTO : BaseDTO where TService : IBaseService<TModel>
{
    protected readonly TService _service;

    public BaseController(TService service, ICustomLogger logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet("GetById")]
    public virtual async Task<ActionResult<OperationResult<TDTO>>> GetByIdAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int id)
    {
        OperationResult<TDTO> result = _service.MapModelDTO<TDTO>(await _service.GetByIdAsync(id));
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

    [HttpPost("Add")]
    public virtual async Task<ActionResult<OperationResult<TDTO>>> AddAsync([Required][FromBody] TDTO dto)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<TDTO>.Fail(currUserIdRes.Errors));

        OperationResult<TModel> modelRes = _service.MapModelDTO<TDTO>(dto);
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<TDTO>.Fail(modelRes.Errors));

        OperationResult<TDTO> result = _service.MapModelDTO<TDTO>(await _service.AddAsync(modelRes.Data!, currUserIdRes.Data));
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

    [HttpPut("Update")]
    public virtual async Task<ActionResult<OperationResult<TDTO>>> UpdateAsync([Required][FromBody] TDTO dto)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<TDTO>.Fail(currUserIdRes.Errors));

        OperationResult<TModel> modelRes = _service.MapModelDTO<TDTO>(dto);
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<TDTO>.Fail(modelRes.Errors));

        OperationResult<TDTO> result = _service.MapModelDTO<TDTO>(await _service.UpdateAsync(modelRes.Data!, currUserIdRes.Data));
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        return Ok(result);
    }

}
