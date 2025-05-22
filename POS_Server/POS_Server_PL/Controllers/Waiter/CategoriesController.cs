using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.DTOs;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Controllers.Waiter.BaseControllers;
using POS_Server_PL.Models.RequestsModels.DTOsRequstsModels.FiltersModels;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace POS_Server_PL.Controllers.Waiter;

public class CategoriesController : SoftDeletableController<CategoryModel, CategoryDTO, ICategoryService>
{
    public CategoriesController(ICategoryService catService, ICustomLogger logger) : base(catService, logger) { }

    [HttpPost("FilterBy")]
    public async Task<ActionResult<OperationResult<List<CategoryDTO>>>> FilterByAsync(
        [Required][FromBody] CategoryDTOFilterModel categoryDTOFilter)
    {
        return await FilterByAsync(CreateFilterPredicate(categoryDTOFilter));
    }

    [HttpPost("FilterByPaged")]
    public async Task<ActionResult<OperationResult<List<CategoryDTO>>>> FilterByPagedAsync(
        [Required][FromBody] CategoryDTOFilterModel categoryDTOFilter,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum)
    {
        return await FilterByPagedAsync(CreateFilterPredicate(categoryDTOFilter), pageQty, pageNum);
    }

    Expression<Func<CategoryModel, bool>> CreateFilterPredicate(CategoryDTOFilterModel categoryDTOFilter)
    {
        return c =>
            (categoryDTOFilter.Name == null || c.Name == categoryDTOFilter.Name) &&
            (categoryDTOFilter.Description == null || c.Description == categoryDTOFilter.Description);
    }

    #region Non action endpoints

    [NonAction]
    public override Task<ActionResult<OperationResult<CategoryDTO>>> AddAsync(
        [FromBody, Required] CategoryDTO dto)
    {
        return base.AddAsync(dto);
    }

    [NonAction]
    public override Task<ActionResult<OperationResult<CategoryDTO>>> UpdateAsync(
        [FromBody, Required] CategoryDTO dto)
    {
        return base.UpdateAsync(dto);
    }

    [NonAction]
    public override Task<ActionResult<OperationResult<int>>> SoftDeleteAsync(
        [FromBody, Range(1, int.MaxValue), Required] int id)
    {
        return base.SoftDeleteAsync(id);
    }

    [NonAction]
    public override Task<ActionResult<OperationResult<CategoryDTO>>> RestoreAsync(
        [FromBody, Range(1, int.MaxValue), Required] int id)
    {
        return base.RestoreAsync(id);
    }

    #endregion

}
