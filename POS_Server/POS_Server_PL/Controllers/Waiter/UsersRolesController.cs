using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using POS_Server_BLL.Interfaces.OtherInterfaces;

namespace POS_Server_PL.Controllers.Waiter;

[Authorize(Roles = "Admin, Waiter")]
[Route("api/Waiter/[controller]")]
[ApiController]
public class UsersRolesController : BaseApiController
{
    readonly IUsersRolesService _usersRolesService;

    public UsersRolesController(IUsersRolesService usersRolesService, ICustomLogger logger) : base(logger)
    {
        _usersRolesService = usersRolesService;
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

}
