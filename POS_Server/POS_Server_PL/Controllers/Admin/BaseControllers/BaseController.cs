using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.BaseInterfaces;
using System.ComponentModel.DataAnnotations;

namespace POS_Server_PL.Controllers.Admin.BaseControllers;

[Authorize(Roles = "Admin")]
[Route("api/Admin/[controller]")]
[ApiController]
public abstract class BaseController<TModel, TService> : BaseApiController
    where TModel : BaseModel where TService : IBaseService<TModel>
{
    protected readonly TService _service;

    protected string ModelName => typeof(TModel).Name;

    public BaseController(TService service, ICustomLogger logger) : base(logger)
    {
        _service = service;
    }

    [HttpGet("GetById")]
    public virtual async Task<ActionResult<OperationResult<TModel>>> GetByIdAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int id)
    {
        OperationResult<TModel> result = await _service.GetByIdAsync(id);
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        _service.RemoveNavigationProps(result.Data);
        return Ok(result);
    }

    [HttpPost("Add")]
    public virtual async Task<ActionResult<OperationResult<TModel>>> AddAsync([Required][FromBody] TModel model)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<TModel>.Fail(currUserIdRes.Errors));

        OperationResult<TModel> result = await _service.AddAsync(model, currUserIdRes.Data);
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        _service.RemoveNavigationProps(result.Data);
        return Ok(result);
    }

    [HttpPut("Update")]
    public virtual async Task<ActionResult<OperationResult<TModel>>> UpdateAsync([Required][FromBody] TModel model)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<TModel>.Fail(currUserIdRes.Errors));

        OperationResult<TModel> result = await _service.UpdateAsync(model, currUserIdRes.Data);
        if (!result.Succeeded || result.Data == null) return HandleInternalFailure(result);
        _service.RemoveNavigationProps(result.Data);
        return Ok(result);
    }

}
