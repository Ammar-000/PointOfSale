using Microsoft.AspNetCore.Mvc;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using Helper.DataContainers;
using POS_Domains.Models;
using Microsoft.AspNetCore.Authorization;
using Helper.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using POS_Server_PL.Models;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.Text;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using POS_Server_PL.Models.RequestsModels;

namespace POS_Server_PL.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LoginController : BaseApiController
{
    readonly IUserService _userService;
    readonly IUsersRolesService _usersRolesService;
    readonly JwtSettingsModel _jwtSettings;

    public LoginController(IUserService userService, IUsersRolesService usersRolesService,
        IOptions<POSSettingsModel> posSettings, ICustomLogger logger)
        : base(logger)
    {
        _userService = userService;
        _usersRolesService = usersRolesService;
        _jwtSettings = posSettings.Value.JwtSettings;
    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult<OperationResult<string>>> LoginAsync([Required][FromBody] LoginRequestModel loginRequest)
    {
        OperationResult<UserModel> userResult = await _userService.CheckUserNameLoginAsync(loginRequest.Username, loginRequest.Password);
        if (!userResult.Succeeded || userResult.Data == null) return Unauthorized(OperationResult<string>.Fail(userResult.Errors));

        OperationResult<string> tokenResult = await GenerateJwtToken(userResult.Data);
        if (!tokenResult.Succeeded || string.IsNullOrEmpty(tokenResult.Data))
        {
            List<string> errors = new() { "User is authorized, but failed to generate JWT token." };
            errors.AddRange(tokenResult.Errors);
            return HandleInternalFailure(OperationResult<string>.Fail(errors));
        }

        return Ok(OperationResult<string>.Success(data: tokenResult.Data));
    }

    async Task<OperationResult<string>> GenerateJwtToken(UserModel user)
    {
        OperationResult<List<string>> rolesResult = await _usersRolesService.GetUserRolesNamesAsync(user.Id);
        if (!rolesResult.Succeeded || rolesResult.Data == null) return OperationResult<string>.Fail(rolesResult.Errors);

        if (_jwtSettings == null || string.IsNullOrWhiteSpace(_jwtSettings.Issuer)
            || string.IsNullOrWhiteSpace(_jwtSettings.Audience) || string.IsNullOrWhiteSpace(_jwtSettings.Key)
            || _jwtSettings.DurationInMinutes <= 0)
            return OperationResult<string>.Fail("Failed to get JWT configurations.");

        List<Claim> claims = new()
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (ClaimTypes.NameIdentifier, user.Id),
            new (ClaimTypes.Name, user.UserName)
        };
        claims.AddRange(rolesResult.Data.Select(role => new Claim(ClaimTypes.Role, role)));

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(_jwtSettings.Key));
        SigningCredentials creds = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(_jwtSettings.DurationInMinutes),
            signingCredentials: creds
        );

        return OperationResult<string>.Success(new JwtSecurityTokenHandler().WriteToken(token));
    }

}