using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.AspNetCore.Mvc;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_PL.Controllers.Admin.BaseControllers;
using POS_Server_PL.Models.RequestsModels.ModelsRequstsModels.FiltersModels;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text;

namespace POS_Server_PL.Controllers.Admin;

public class ProductsController : SoftDeletableController<ProductModel, IProductService>
{
    string[] allowedImgExts = new[] { ".jpg", ".jpeg", ".png", ".webp" };

    public ProductsController(IProductService prodService, ICustomLogger logger) : base(prodService, logger) { }

    [HttpPost("FilterBy")]
    public async Task<ActionResult<OperationResult<List<ProductModel>>>> FilterByAsync(
        [Required][FromBody] ProductFilterModel productFilter,
        [FromQuery] bool includeInActive = false)
    {
        return await FilterByAsync(CreateFilterPredicate(productFilter), includeInActive);
    }

    [HttpPost("FilterByPaged")]
    public async Task<ActionResult<OperationResult<List<ProductModel>>>> FilterByPagedAsync(
        [Required][FromBody] ProductFilterModel productFilter,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageQty,
        [Required][FromQuery][Range(1, int.MaxValue)] int pageNum,
        [FromQuery] bool includeInActive = false)
    {
        return await FilterByPagedAsync(CreateFilterPredicate(productFilter), pageQty, pageNum, includeInActive);
    }

    [HttpGet("GetImageUrl")]
    public async Task<ActionResult<OperationResult<string>>> GetImageUrlAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int id)
    {
        OperationResult<string> result = await _service.GetImageUrlAsync(id);
        return result.Succeeded && !string.IsNullOrWhiteSpace(result.Data) ? result : HandleInternalFailure(result);
    }

    [HttpPost("AddImage")]
    public async Task<ActionResult<OperationResult<ProductModel>>> AddImageAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int id, [Required] IFormFile image)
    {
        if (image == null || image.Length == 0)
            return BadRequest(OperationResult<ProductModel>.Fail("Invalid image file."));

        string? fileExt = Path.GetExtension(image.FileName).ToLower();
        if (string.IsNullOrWhiteSpace(fileExt) || !allowedImgExts.Contains(fileExt))
            return BadRequest(OperationResult<string>.Fail("Invalid file format."));

        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<ProductModel>.Fail(currUserIdRes.Errors));

        using Stream imgStream = image.OpenReadStream();
        OperationResult<ProductModel> result = await _service.AddImageAsync(id, imgStream, image.FileName, currUserIdRes.Data);
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpPost("UpdateImage")]
    public async Task<ActionResult<OperationResult<ProductModel>>> UpdateImageAsync(
    [Required][FromQuery][Range(1, int.MaxValue)] int id, [Required] IFormFile image)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<ProductModel>.Fail(currUserIdRes.Errors));

        if (image == null || image.Length == 0)
            return BadRequest(OperationResult<ProductModel>.Fail("Invalid image file."));

        string? fileExt = Path.GetExtension(image.FileName).ToLower();
        if (string.IsNullOrWhiteSpace(fileExt) || !allowedImgExts.Contains(fileExt))
            return BadRequest(OperationResult<string>.Fail("Invalid file format."));

        using Stream imgStream = image.OpenReadStream();
        OperationResult<ProductModel> result = await _service.UpdateImageAsync(id, imgStream, image.FileName, currUserIdRes.Data);
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    [HttpGet("DeleteImage")]
    public async Task<ActionResult<OperationResult<ProductModel>>> DeleteImageAsync(
        [Required][FromQuery][Range(1, int.MaxValue)] int id)
    {
        OperationResult<string> currUserIdRes = GetCurrentUserId();
        if (!currUserIdRes.Succeeded || currUserIdRes.Data == null)
            return HandleInternalFailure(OperationResult<ProductModel>.Fail(currUserIdRes.Errors));

        OperationResult<ProductModel> result = await _service.DeleteImageAsync(id, currUserIdRes.Data);
        return result.Succeeded ? Ok(result) : HandleInternalFailure(result);
    }

    Expression<Func<ProductModel, bool>> CreateFilterPredicate(ProductFilterModel productFilter)
    {
        return p =>
            (productFilter.Name == null || p.Name == productFilter.Name) &&
            (productFilter.Price == null || p.Price == productFilter.Price) &&
            (productFilter.Description == null || p.Description == productFilter.Description) &&
            (productFilter.CategoryId == null || p.CategoryId == productFilter.CategoryId) &&
            (productFilter.CreatedAt == null || p.CreatedAt == productFilter.CreatedAt) &&
            (productFilter.CreatedBy == null || p.CreatedBy == productFilter.CreatedBy) &&
            (productFilter.UpdatedAt == null || p.UpdatedAt == productFilter.UpdatedAt) &&
            (productFilter.UpdatedBy == null || p.UpdatedBy == productFilter.UpdatedBy);
    }

}
