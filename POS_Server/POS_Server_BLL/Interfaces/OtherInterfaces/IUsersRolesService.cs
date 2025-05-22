using Helper.DataContainers;
using POS_Domains.Models;

namespace POS_Server_BLL.Interfaces.OtherInterfaces;

public interface IUsersRolesService
{
    public Task<OperationResult<List<string>>> GetUserRolesNamesAsync(string userId);
    public Task<OperationResult<List<UserModel>>> GetUsersInRoleAsync(string roleId);
    public Task<OperationResult> IsInRoleAsync(string userId, string roleId);
    public Task<OperationResult> AddToRoleAsync(string userId, string roleId, string currUserId);
    public Task<OperationResult> RemoveFromRoleAsync(string userId, string roleId, string currUserId);
}
