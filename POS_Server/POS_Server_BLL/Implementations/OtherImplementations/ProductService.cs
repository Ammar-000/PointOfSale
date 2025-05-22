using AutoMapper;
using Helper.DataContainers;
using Helper.Interfaces;
using POS_Domains.Models;
using POS_Server_BLL.Implementations.BaseImplementations;
using POS_Server_BLL.Interfaces.BaseInterfaces;
using POS_Server_BLL.Interfaces.OtherInterfaces;
using POS_Server_DAL.Repositories.Interfaces;

namespace POS_Server_BLL.Implementations.OtherImplementations;

public class ProductService : SoftDeletableService<ProductModel>, IProductService
{
    bool _allowSaveImgSubPath = false;
    readonly IImageService<ProductModel> _imageService;

    public ProductService(IImageService<ProductModel> imageService, ISoftDeletableRepository<ProductModel> repository,
        IEntityValidator<ProductModel> validator, IMapper mapper, ICustomLogger logger, IUserService userService)
        : base(repository, validator, mapper, logger, userService)
    {
        _imageService = imageService;
        _imageService.InitImageServiceDelegates(GetSubDirectory, GetEntityByIdAsync,
            ChkHasImage, GetImageSubPath, SaveImageSubPathAsync, RemoveImgSubPathAsync);
    }

    #region IProductService methods
    public override async Task<OperationResult<ProductModel>> AddAsync(ProductModel product, string currUserId)
    {
        product.ImageSubPath = null;
        return await base.AddAsync(product, currUserId);
    }

    public async override Task<OperationResult<ProductModel>> UpdateAsync(ProductModel product, string currUserId)
    {
        if (!_allowSaveImgSubPath)
        {
            if (product.Id == null) return OperationResult<ProductModel>.Fail("Id can't be null.");
            OperationResult<ProductModel> origProdRes = await GetByIdAsync(product.Id.Value!);
            if (!origProdRes.Succeeded || origProdRes.Data == null)
                return OperationResult<ProductModel>.Fail("Failed to fetch original Product.");
            product.ImageSubPath = origProdRes.Data.ImageSubPath;
        }
        return await base.UpdateAsync(product, currUserId);
    }

    public override void RemoveNavigationProps(ProductModel product)
    {
        product.Category = null;
    }
    #endregion

    #region IImageService methods

    public void InitImageServiceDelegates(Func<string> getSubDirectory,
        Func<int, Task<OperationResult<ProductModel>>> getEntityByIdAsync, Predicate<ProductModel> chkHasImage,
        Func<ProductModel, string?> getImageSubPath, Func<ProductModel, string, string, Task<OperationResult<ProductModel>>> saveImageSubPathAsync,
        Func<ProductModel, string, Task<OperationResult<ProductModel>>> removeImgSubPathAsync)
    {
        return;
    }

    public async Task<OperationResult<string>> GetImageUrlAsync(int entityId)
    {
        return await _imageService.GetImageUrlAsync(entityId);
    }

    public async Task<OperationResult<ProductModel>> AddImageAsync(
        int entityId, Stream imageStream, string originalFileName, string currUserId)
    {
        OperationResult ChkCurrUserIdRes = await CheckCurrUser(currUserId);
        if (!ChkCurrUserIdRes.Succeeded) return OperationResult<ProductModel>.Fail(ChkCurrUserIdRes.Errors);

        return await _imageService.AddImageAsync(entityId, imageStream, originalFileName, currUserId);
    }

    public async Task<OperationResult<ProductModel>> UpdateImageAsync(
        int entityId, Stream newImageStream, string originalNewFileName, string currUserId)
    {
        OperationResult ChkCurrUserIdRes = await CheckCurrUser(currUserId);
        if (!ChkCurrUserIdRes.Succeeded) return OperationResult<ProductModel>.Fail(ChkCurrUserIdRes.Errors);

        return await _imageService.UpdateImageAsync(entityId, newImageStream, originalNewFileName, currUserId);
    }

    public async Task<OperationResult<ProductModel>> DeleteImageAsync(int entityId, string currUserId)
    {
        OperationResult ChkCurrUserIdRes = await CheckCurrUser(currUserId);
        if (!ChkCurrUserIdRes.Succeeded) return OperationResult<ProductModel>.Fail(ChkCurrUserIdRes.Errors);

        return await _imageService.DeleteImageAsync(entityId, currUserId);
    }

    #endregion

    #region IImageService delegate methods

    string GetSubDirectory()
    {
        return "Products";
    }

    async Task<OperationResult<ProductModel>> GetEntityByIdAsync(int productId)
    {
        return await GetByIdAsync(productId);
    }

    bool ChkHasImage(ProductModel product)
    {
        return !string.IsNullOrWhiteSpace(product.ImageSubPath);
    }

    string? GetImageSubPath(ProductModel product)
    {
        return string.IsNullOrWhiteSpace(product.ImageSubPath) ? null : product.ImageSubPath;
    }

    async Task<OperationResult<ProductModel>> SaveImageSubPathAsync(ProductModel product, string imageSubPath, string currUserId)
    {
        _allowSaveImgSubPath = true;
        product.ImageSubPath = imageSubPath;
        OperationResult<ProductModel> result = await UpdateAsync(product, currUserId);
        _allowSaveImgSubPath = false;
        return result;
    }

    async Task<OperationResult<ProductModel>> RemoveImgSubPathAsync(ProductModel product, string currUserId)
    {
        _allowSaveImgSubPath = true;
        product.ImageSubPath = null;
        OperationResult<ProductModel> result = await UpdateAsync(product, currUserId);
        _allowSaveImgSubPath = false;
        return result;
    }

    #endregion

}
