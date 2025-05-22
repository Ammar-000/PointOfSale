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

public class ProductsController : SoftDeletableController<ProductModel, ProductDTO, IProductService>
{
    public ProductsController(IProductService prodService, ICustomLogger logger) : base(prodService, logger) { }

    [HttpPost("FilterBy")]
    public async Task<ActionResult<OperationResult<List<ProductDTO>>>> FilterByAsync(
        [Required][FromBody] ProductDTOFilterModel productDTOFilter)
    {
        return await FilterByAsync(CreateFilterPredicate(productDTOFilter));
    }

    [HttpPost("FilterByPaged")]
    public async Task<ActionResult<OperationResult<List<ProductDTO>>>> FilterByPagedAsync(
        [Required][FromBody] ProductDTOFilterModel productDTOFilter,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum)
    {
        return await FilterByPagedAsync(CreateFilterPredicate(productDTOFilter), pageQty, pageNum);
    }

    [HttpGet("GetImageUrl")]
    public async Task<ActionResult<OperationResult<string>>> GetImageUrlAsync(
    [Required][FromQuery][Range(1, int.MaxValue)] int id)
    {
        OperationResult<string> result = await _service.GetImageUrlAsync(id);
        return result.Succeeded && !string.IsNullOrWhiteSpace(result.Data) ? result : HandleInternalFailure(result);
    }

    Expression<Func<ProductModel, bool>> CreateFilterPredicate(ProductDTOFilterModel productDTOFilter)
    {
        return p =>
            (productDTOFilter.Name == null || p.Name == productDTOFilter.Name) &&
            (productDTOFilter.Price == null || p.Price == productDTOFilter.Price) &&
            (productDTOFilter.Description == null || p.Description == productDTOFilter.Description) &&
            (productDTOFilter.CategoryId == null || p.CategoryId == productDTOFilter.CategoryId);
    }

    #region Non action endpoints

    [NonAction]
    public override Task<ActionResult<OperationResult<ProductDTO>>> AddAsync(
        [FromBody, Required] ProductDTO dto)
    {
        return base.AddAsync(dto);
    }

    [NonAction]
    public override Task<ActionResult<OperationResult<ProductDTO>>> UpdateAsync(
        [FromBody, Required] ProductDTO dto)
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
    public override Task<ActionResult<OperationResult<ProductDTO>>> RestoreAsync(
        [FromBody, Range(1, int.MaxValue), Required] int id)
    {
        return base.RestoreAsync(id);
    }
    #endregion

}
