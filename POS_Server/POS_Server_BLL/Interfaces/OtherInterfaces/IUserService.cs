using Helper.DataContainers;
using POS_Domains.DTOs;
using POS_Domains.Models;
using System.Linq.Expressions;

namespace POS_Server_BLL.Interfaces.OtherInterfaces;

public interface IUserService
{
    #region Basic methods
    public Task<OperationResult<int>> CountAsync(bool includeInActive = false);
    public Task<OperationResult<List<UserModel>>> GetAllAsync(bool includeInActive = false);
    public Task<OperationResult<List<UserModel>>> FilterByAsync(Expression<Func<UserModel, bool>> predicate, bool includeInActive = false);
    public Task<OperationResult<List<UserModel>>> GetAllPagedAsync(int pageQty, int pageNum, bool includeInActive = false);
    public Task<OperationResult<List<UserModel>>> FilterByPagedAsync(Expression<Func<UserModel, bool>> predicate,
        int pageQty, int pageNum, bool includeInActive);
    public Task<OperationResult> CheckExistByAsync(Expression<Func<UserModel, bool>> predicate, bool includeInActive = false);
    public Task<OperationResult<UserModel>> GetByIdAsync(string id);
    public Task<OperationResult<UserModel>> AddAsync(UserModel user, string password, string? currUserId);
    public Task<OperationResult<UserModel>> UpdateAsync(UserModel user, string currUserId);
    public Task<OperationResult<string>> SoftDeleteAsync(string userId, string currUserId);
    public Task<OperationResult<UserModel>> RestoreAsync(string userId, string currUserId);
    #endregion

    #region Login management methods
    public Task<OperationResult<UserModel>> CheckUserNameLoginAsync(string userName, string password);
    public Task<OperationResult<UserModel>> CheckEmailLoginAsync(string email, string password);
    public Task<OperationResult> LockAsync(string userId, string currUserId);
    public Task<OperationResult> UnLockAsync(string userId, string currUserId);
    #endregion

    #region Password management methods
    public Task<OperationResult<UserModel>> ChangePasswordAsync(string userId, string currentPassword, string newPassword, string currUserId);
    public Task<OperationResult<string>> GeneratePasswordResetTokenAsync(string userId);
    public Task<OperationResult<UserModel>> ResetPasswordAsync(string userId, string token, string newPassword, string currUserId);
    #endregion

    #region Map methods
    public OperationResult<UserModel> MapModelDTO(UserDTO dto);
    public OperationResult<UserDTO> MapModelDTO(UserModel model);
    public OperationResult<List<UserModel>> MapModelDTO(List<UserDTO> dtos);
    public OperationResult<List<UserDTO>> MapModelDTO(List<UserModel> models);
    public OperationResult<UserModel> MapModelDTO(OperationResult<UserDTO> dtoRes);
    public OperationResult<UserDTO> MapModelDTO(OperationResult<UserModel> modelRes);
    public OperationResult<List<UserModel>> MapModelDTO(OperationResult<List<UserDTO>> dtosRes);
    public OperationResult<List<UserDTO>> MapModelDTO(OperationResult<List<UserModel>> modelsRes);
    #endregion
}
