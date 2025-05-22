using Helper.DataContainers;
using Helper.Interfaces;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using System.Text;

namespace POS_Server_BLL.Implementations.OtherImplementations;

public class Seeder : ISeeder
{
    readonly string LayerName = "BLL";
    readonly IUserService _userService;
    readonly IRoleService _RoleService;
    readonly IUsersRolesService _UsersRolesService;
    readonly ICustomLogger _logger;

    public Seeder(IUserService userService, IRoleService roleService, IUsersRolesService UsersRolesService, ICustomLogger logger)
    {
        _userService = userService;
        _RoleService = roleService;
        _UsersRolesService = UsersRolesService;
        _logger = logger;
    }

    public async Task<OperationResult> SeedData()
    {
        string failMsg = "Failed to seed data.";

        OperationResult result = await SeedUsersAndRoles();
        if (!result.Succeeded)
        {
            result.AddError(failMsg);
            LogError(LayerName, null, CombineMessages(result.Errors.ToArray()));
            return result;
        }

        return OperationResult.Success();
    }

    async Task<OperationResult> SeedUsersAndRoles()
    {
        string roleAdmin = "Admin", roleWaiter = "Waiter";
        Dictionary<string, string> rolesNamesAndDesc = new(2)
        {
            {roleAdmin,"System administrator." },
            {roleWaiter,"Serves customers and handles orders." }
        };

        OperationResult<UserModel> seedUserRes = await SeedUser();
        if (!seedUserRes.Succeeded) return OperationResult.Fail(seedUserRes.Errors);

        OperationResult<List<RoleModel>> seedRolesRes = await SeedRoles(seedUserRes.Data!.Id, rolesNamesAndDesc);
        if (!seedRolesRes.Succeeded) return OperationResult.Fail(seedRolesRes.Errors);

        OperationResult seedUserRoleRes = await SeedAdmin(seedUserRes.Data!, seedRolesRes.Data!.First(role => role.Name == roleAdmin));

        return seedUserRoleRes;
    }

    async Task<OperationResult<UserModel>> SeedUser()
    {
        string failMsg = "Failed to seed users.";

        OperationResult<List<UserModel>> chkUsersRes = await _userService.FilterByPagedAsync(u => true, 1, 1, false);
        if (!chkUsersRes.Succeeded || chkUsersRes.Data == null)
        {
            chkUsersRes.AddError(failMsg);
            return OperationResult<UserModel>.Fail(chkUsersRes.Errors);
        }
        if (chkUsersRes.Data.Count > 0) return OperationResult<UserModel>.Success(chkUsersRes.Data[0]);

        UserModel firstUser = new()
        {
            FirstName = "Ammar1",
            LastName = "Tawfiq",
            UserName = "Ammar_1",
            Email = "ammar1@gmail.com",
            PhoneNumber = "7335629291"
        };

        OperationResult<UserModel> userRes = await _userService.AddAsync(firstUser, "0000001", null);
        if (!userRes.Succeeded) userRes.AddError(failMsg);
        else if (userRes.Data == null)
        {
            userRes.Message = CombineMessages(userRes.Message, $"User '{firstUser.UserName}' has been seeded successfully, but failed to fetch it.");
            userRes.AddError($"User '{firstUser.UserName}' has been seeded successfully, but failed to fetch it.");
        }

        return userRes;
    }

    async Task<OperationResult<List<RoleModel>>> SeedRoles(string userId, Dictionary<string, string> rolesNamesAndDesc)
    {
        string failMsg = "Failed to seed roles.";
        List<RoleModel> roles = new();
        OperationResult<RoleModel> result;

        foreach (string roleName in rolesNamesAndDesc.Keys)
        {
            result = await SeedRole(userId, roleName, rolesNamesAndDesc[roleName]);
            if (!result.Succeeded || result.Data == null)
            {
                result.AddError(failMsg);
                return OperationResult<List<RoleModel>>.Fail(result.Errors);
            }
            roles.Add(result.Data);
        }

        return OperationResult<List<RoleModel>>.Success(roles);
    }

    async Task<OperationResult<RoleModel>> SeedRole(string userId, string roleName, string? roleDescription)
    {
        OperationResult<List<RoleModel>> chkRoleRes = await _RoleService.FilterByPagedAsync(role => role.Name == roleName, 1, 1);
        if (!chkRoleRes.Succeeded || chkRoleRes.Data == null) return OperationResult<RoleModel>.Fail(chkRoleRes.Errors);
        if (chkRoleRes.Data.Count > 0) return OperationResult<RoleModel>.Success(chkRoleRes.Data[0]);

        RoleModel role = new()
        {
            Name = roleName,
            Description = roleDescription
        };
        OperationResult<RoleModel> addRoleRes = await _RoleService.AddAsync(role, userId);
        if (addRoleRes.Succeeded && addRoleRes.Data == null)
        {
            addRoleRes.Message = CombineMessages(addRoleRes.Message, $"Role '{roleName}' has been seeded successfully, but failed to fetch it.");
            addRoleRes.AddError($"Role {roleName} has been seeded successfully, but failed to fetch it.");
        }
        return addRoleRes;
    }

    public async Task<OperationResult> SeedAdmin(UserModel user, RoleModel roleAdmin)
    {
        string failMsg = "Failed to seed admin.";

        OperationResult<List<UserModel>> adminUsersRes = await _UsersRolesService.GetUsersInRoleAsync(roleAdmin.Id);
        if (!adminUsersRes.Succeeded || adminUsersRes.Data == null)
        {
            adminUsersRes.AddError(failMsg);
            return OperationResult.Fail(adminUsersRes.Errors);
        }
        if (adminUsersRes.Data.Count > 0) return OperationResult.Success();

        OperationResult result = await _UsersRolesService.AddToRoleAsync(user.Id, roleAdmin.Id, user.Id);
        return result;
    }

    string CombineMessages(params string[] messages)
    {
        StringBuilder sb = new();
        foreach (string message in messages) if (!string.IsNullOrWhiteSpace(message)) sb.AppendLine(message);
        if (sb.Length > 2) sb.Replace("\r\n", "", sb.Length - 2, 2);
        return sb.ToString();
    }

    protected void LogError(string layer, Exception? ex, string customMsg)
    {
        _logger.LogError(layer, ex, customMsg);
    }

}
