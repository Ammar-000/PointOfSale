using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace POS_Server_PL.Controllers;

[ApiController]
public class BaseApiController : ControllerBase
{
    protected readonly ICustomLogger _logger;

    public BaseApiController(ICustomLogger logger)
    {
        _logger = logger;
    }

    protected virtual OperationResult<string> GetCurrentUserId()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value.Trim();
        if (!string.IsNullOrWhiteSpace(userId)) return OperationResult<string>.Success(data: userId);

        _logger.LogError("PL", null, "$GetCurrentUserId failed: ClaimTypes.NameIdentifier not found.");
        return OperationResult<string>.Fail("Failed to get current user Id.");
    }

    protected virtual ActionResult HandleInternalFailure(OperationResult result)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }

    protected virtual ActionResult HandleInternalFailure<T>(OperationResult<T> result)
    {
        return StatusCode(StatusCodes.Status500InternalServerError, result);
    }
}
