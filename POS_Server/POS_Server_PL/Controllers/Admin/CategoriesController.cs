using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Controllers.Admin.BaseControllers;
using POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.FiltersModels;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace POS_Server_PL.Controllers.Admin;

public class CategoriesController : SoftDeletableController<CategoryModel, ICategoryService>
{
    public CategoriesController(ICategoryService catService, ICustomLogger logger) : base(catService, logger) { }

    [HttpPost("FilterBy")]
    public async Task<ActionResult<OperationResult<List<CategoryModel>>>> FilterByAsync(
        [Required][FromBody] CategoryFilterModel categoryFilter,
        [FromQuery] bool includeInActive = false)
    {
        return await FilterByAsync(CreateFilterPredicate(categoryFilter), includeInActive);
    }

    [HttpPost("FilterByPaged")]
    public async Task<ActionResult<OperationResult<List<CategoryModel>>>> FilterByPagedAsync(
        [Required][FromBody] CategoryFilterModel categoryFilter,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum,
        [FromQuery] bool includeInActive = false)
    {
        return await FilterByPagedAsync(CreateFilterPredicate(categoryFilter), pageQty, pageNum, includeInActive);
    }

    Expression<Func<CategoryModel, bool>> CreateFilterPredicate(CategoryFilterModel categoryFilter)
    {
        return c =>
            (categoryFilter.Name == null || c.Name == categoryFilter.Name) &&
            (categoryFilter.Description == null || c.Description == categoryFilter.Description) &&
            (categoryFilter.CreatedAt == null || c.CreatedAt == categoryFilter.CreatedAt) &&
            (categoryFilter.CreatedBy == null || c.CreatedBy == categoryFilter.CreatedBy) &&
            (categoryFilter.UpdatedAt == null || c.UpdatedAt == categoryFilter.UpdatedAt) &&
            (categoryFilter.UpdatedBy == null || c.UpdatedBy == categoryFilter.UpdatedBy);
    }

}
