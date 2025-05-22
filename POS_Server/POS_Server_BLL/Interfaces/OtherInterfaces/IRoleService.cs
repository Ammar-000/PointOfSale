using Helper.DataContainers;
using POS_Domains.DTOs;
using POS_Domains.Models;
using System.Linq.Expressions;

namespace POS_Server_BLL.Interfaces.OtherInterfaces;

public interface IRoleService
{
    #region Basic methods

    public Task<OperationResult<int>> CountAsync();
    public Task<OperationResult<List<RoleModel>>> GetAllAsync();
    public Task<OperationResult<List<RoleModel>>> FilterByAsync(Expression<Func<RoleModel, bool>> predicate);
    public Task<OperationResult<List<RoleModel>>> GetAllPagedAsync(int pageQty, int pageNum);
    public Task<OperationResult<List<RoleModel>>> FilterByPagedAsync(
        Expression<Func<RoleModel, bool>> predicate, int pageQty, int pageNum);
    public Task<OperationResult> CheckExistByAsync(Expression<Func<RoleModel, bool>> predicate);
    public Task<OperationResult<RoleModel>> GetByIdAsync(string id);
    public Task<OperationResult<RoleModel>> AddAsync(RoleModel role, string currUserId);
    public Task<OperationResult<RoleModel>> UpdateAsync(RoleModel role, string currUserId);
    public Task<OperationResult<string>> HardDeleteAsync(string roleId, string currUserId);

    #endregion

    #region Map methods
    public OperationResult<RoleModel> MapModelDTO(RoleDTO dto);
    public OperationResult<RoleDTO> MapModelDTO(RoleModel model);
    public OperationResult<List<RoleModel>> MapModelDTO(List<RoleDTO> dtos);
    public OperationResult<List<RoleDTO>> MapModelDTO(List<RoleModel> models);
    public OperationResult<RoleModel> MapModelDTO(OperationResult<RoleDTO> dtoRes);
    public OperationResult<RoleDTO> MapModelDTO(OperationResult<RoleModel> modelRes);
    public OperationResult<List<RoleModel>> MapModelDTO(OperationResult<List<RoleDTO>> dtosRes);
    public OperationResult<List<RoleDTO>> MapModelDTO(OperationResult<List<RoleModel>> modelsRes);
    #endregion
}
