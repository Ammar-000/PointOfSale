using Helper.DataContainers;
using POS_Domains.DTOs;
using POS_Domains.Models;

namespace POS_Server_BLL.Interfaces.BaseInterfaces;

public interface IBaseService<TModel> where TModel : BaseModel
{
    #region CRUD methods
    public Task<OperationResult<TModel>> GetByIdAsync(int id);
    public Task<OperationResult<TModel>> AddAsync(TModel entity, string currUserId);
    public Task<OperationResult<TModel>> UpdateAsync(TModel entity, string currUserId);
    #endregion

    #region Map mehods
    public OperationResult<TModel> MapModelDTO<TDTO>(TDTO dto) where TDTO : BaseDTO;
    public OperationResult<TDTO> MapModelDTO<TDTO>(TModel model) where TDTO : BaseDTO;
    public OperationResult<List<TModel>> MapModelDTO<TDTO>(List<TDTO> dtos) where TDTO : BaseDTO;
    public OperationResult<List<TDTO>> MapModelDTO<TDTO>(List<TModel> models) where TDTO : BaseDTO;
    public OperationResult<TModel> MapModelDTO<TDTO>(OperationResult<TDTO> dtoRes) where TDTO : BaseDTO;
    public OperationResult<TDTO> MapModelDTO<TDTO>(OperationResult<TModel> modelRes) where TDTO : BaseDTO;
    public OperationResult<List<TModel>> MapModelDTO<TDTO>(OperationResult<List<TDTO>> dtosRes) where TDTO : BaseDTO;
    public OperationResult<List<TDTO>> MapModelDTO<TDTO>(OperationResult<List<TModel>> modelsRes) where TDTO : BaseDTO;
    #endregion

    #region Helper methods
    public void RemoveNavigationProps(TModel entity);
    #endregion
}
