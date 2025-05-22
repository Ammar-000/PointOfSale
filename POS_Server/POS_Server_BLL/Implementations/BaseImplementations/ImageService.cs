using Helper.DataContainers;
using POS_Domains.Models;
using POS_Server_BLL.Interfaces.BaseInterfaces;
using POS_Server_DAL.Repositories.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace POS_Server_BLL.Implementations.BaseImplementations;

public class ImageService<T> : IImageService<T> where T : BaseModel
{
    readonly IImagesRepository _imagesRepository;
    bool _areDelegatesInitialized = false;
    readonly string _delegatesNotInitializedMsg;

    string EntityName = typeof(T).Name;

    #region Delegates

    /// <summary>
    /// Returns: sub directory.
    /// </summary>
    Func<string> GetSubDirectory { get; set; }

    /// <summary>
    /// Takes: entity id.
    /// Returns: entity.
    /// </summary>
    Func<int, Task<OperationResult<T>>> GetEntityByIdAsync { get; set; }

    /// <summary>
    /// Takes: entity.
    /// Returns: hasImage.
    /// </summary>
    Predicate<T> ChkHasImage { get; set; }

    /// <summary>
    /// Takes: entity.
    /// Returns: image sub path.
    /// </summary>
    Func<T, string?> GetImageSubPath { get; set; }

    /// <summary>
    /// Takes: entity, new image sub path, current user Id.
    /// Returns: updated entity.
    /// </summary>
    Func<T, string, string, Task<OperationResult<T>>> SaveImageSubPathAsync { get; set; }

    /// <summary>
    /// Takes: entity, current user Id.
    /// Returns: updated entity.
    /// </summary>
    Func<T, string, Task<OperationResult<T>>> RemoveImgSubPathAsync { get; set; }

    #endregion

    public ImageService(IImagesRepository imagesRepository)
    {
        _imagesRepository = imagesRepository;
        _delegatesNotInitializedMsg = $"Service is not initialized yet, it's must be " +
            $"initialized through{nameof(InitImageServiceDelegates)}";
    }

    #region Main methods

    public void InitImageServiceDelegates(Func<string> getSubDirectory, Func<int, Task<OperationResult<T>>> getEntityByIdAsync,
        Predicate<T> chkHasImage, Func<T, string?> getImageSubPath, Func<T, string, string, Task<OperationResult<T>>> saveImageSubPathAsync,
        Func<T, string, Task<OperationResult<T>>> removeImgSubPathAsync)
    {
        if (getSubDirectory == null || getEntityByIdAsync == null || chkHasImage == null ||
            getImageSubPath == null || saveImageSubPathAsync == null || removeImgSubPathAsync == null) return;

        GetSubDirectory = getSubDirectory;
        GetEntityByIdAsync = getEntityByIdAsync;
        ChkHasImage = chkHasImage;
        GetImageSubPath = getImageSubPath;
        SaveImageSubPathAsync = saveImageSubPathAsync;
        RemoveImgSubPathAsync = removeImgSubPathAsync;

        _areDelegatesInitialized = true;
    }

    public async Task<OperationResult<string>> GetImageUrlAsync(int entityId)
    {
        string failMsg = $"Failed to get url for image of {EntityName} with id {entityId}.";
        if (!_areDelegatesInitialized) return OperationResult<string>.Fail(_delegatesNotInitializedMsg + "\r\n" + failMsg);

        OperationResult<T> entityRes = await GetEntityByIdAsync(entityId);
        if (!entityRes.Succeeded || entityRes.Data == null)
        {
            entityRes.AddError(failMsg);
            return OperationResult<string>.Fail(entityRes.Errors);
        }

        string? imageSubPath = GetImageSubPath(entityRes.Data);
        if (string.IsNullOrWhiteSpace(imageSubPath))
            return OperationResult<string>.Fail($"{EntityName} with id {entityId} doesn't have image.\r\n" + failMsg);

        imageSubPath = NormalizePath(imageSubPath);
        return await _imagesRepository.GetImageUrlAsync(imageSubPath);
    }

    public async Task<OperationResult<T>> AddImageAsync(
        int entityId, Stream imageStream, string originalFileName, string currUserId)
    {
        string failMsg = $"Failed to add image to {EntityName} with id {entityId}.";
        if (!_areDelegatesInitialized) return OperationResult<T>.Fail(_delegatesNotInitializedMsg + "\r\n" + failMsg);

        OperationResult<T> entityRes = await GetEntityByIdAsync(entityId);
        if (!entityRes.Succeeded || entityRes.Data == null)
        {
            entityRes.AddError(failMsg);
            return OperationResult<T>.Fail(entityRes.Errors);
        }

        bool hasImage = ChkHasImage(entityRes.Data);
        if (hasImage) return OperationResult<T>.Fail(
            $"{EntityName} with Id {entityId} already has image, update it if you want to change it.");

        string imgFileName = GenerateImageFileName(entityId, originalFileName);

        OperationResult<string> AddRepoRes = await _imagesRepository.AddImageAsync(imageStream, imgFileName, GetSubDirectory());
        if (!AddRepoRes.Succeeded)
        {
            AddRepoRes.AddError(failMsg);
            return OperationResult<T>.Fail(AddRepoRes.Errors);
        }
        if (string.IsNullOrWhiteSpace(AddRepoRes.Data)) return OperationResult<T>.Success(null, CombineMessages(AddRepoRes.Message,
            $"Image has been added successfully, but failed to get imageSubPath and update imageSubPath " +
                $"property of {EntityName}, Errors:\r\n" + string.Join("\r\n", AddRepoRes.Errors)));
        string imageSubPath = AddRepoRes.Data;

        OperationResult<T> saveImgSubPathRes = await SaveImageSubPathAsync(entityRes.Data, imageSubPath, currUserId);
        if (!saveImgSubPathRes.Succeeded) return OperationResult<T>.Success(
            null, $"Image has added successfully, but failed to update imageSubPath property of {EntityName}, Errors:\r\n" +
                string.Join("\r\n", saveImgSubPathRes.Errors));
        if (saveImgSubPathRes.Data == null) saveImgSubPathRes.Message = CombineMessages(saveImgSubPathRes.Message, $"Image has been " +
            $"added successfully, but failed to fetch updated {EntityName}, Errors:\r\n" + string.Join("\r\n", saveImgSubPathRes.Errors));

        return saveImgSubPathRes;
    }

    public async Task<OperationResult<T>> UpdateImageAsync(
        int entityId, Stream newImageStream, string originalNewFileName, string currUserId)
    {
        string failMsg = $"Failed to update image of {EntityName} with id {entityId}.";
        if (!_areDelegatesInitialized) return OperationResult<T>.Fail(_delegatesNotInitializedMsg + "\r\n" + failMsg);
        string message = string.Empty;

        OperationResult<T> result = await DeleteImageAsync(entityId, currUserId);
        if (!result.Succeeded)
        {
            result.AddError(failMsg);
            return result;
        }
        message = CombineMessages(message, result.Message);

        result = await AddImageAsync(entityId, newImageStream, originalNewFileName, currUserId);
        if (!result.Succeeded)
        {
            result.Errors.Add(message);
            result.AddError("Old image has been deleted successfully, but failed to add new one.");
            result.AddError(failMsg);
            return result;
        }
        if (result.Data == null) result.Message = CombineMessages(result.Message, message,
            $"Image has been updadted successfully, but failed to fetch {EntityName}.");

        return result;
    }

    public async Task<OperationResult<T>> DeleteImageAsync(int entityId, string currUserId)
    {
        string failMsg = $"Failed to delete image of {EntityName} with id {entityId}.";
        string message = string.Empty;
        if (!_areDelegatesInitialized) return OperationResult<T>.Fail(_delegatesNotInitializedMsg + "\r\n" + failMsg);

        OperationResult<T> entityRes = await GetEntityByIdAsync(entityId);
        if (!entityRes.Succeeded || entityRes.Data == null)
        {
            entityRes.AddError(failMsg);
            return OperationResult<T>.Fail(entityRes.Errors);
        }

        string? imageSubPath = GetImageSubPath(entityRes.Data);
        if (string.IsNullOrWhiteSpace(imageSubPath))
        {
            entityRes.Message = CombineMessages(entityRes.Message, $"{EntityName} with Id {entityId} already doesn't have image.");
            return entityRes;
        }
        imageSubPath = NormalizePath(imageSubPath);

        OperationResult delResult = await _imagesRepository.DeleteImageAsync(imageSubPath);
        if (!delResult.Succeeded)
        {
            delResult.AddError(failMsg);
            return OperationResult<T>.Fail(delResult.Errors);
        }
        message = CombineMessages(message, delResult.Message);

        OperationResult<T> removeResult = await RemoveImgSubPathAsync(entityRes.Data, currUserId);
        removeResult.Message = CombineMessages(message, removeResult.Message);
        if (!removeResult.Succeeded) return OperationResult<T>.Success(null, CombineMessages(removeResult.Message,
            $"Image has been deleted successfully, but failed to update imageSubPath property of {EntityName}, Errors:\r\n"
                + string.Join("\r\n", removeResult.Errors)));
        if (removeResult.Data == null) removeResult.Message = CombineMessages(removeResult.Message,
            $"Image has been deleted successfully, and imageSubPath property of {EntityName} " +
                $"has been updated successfully, but failed to fetch {EntityName}, Errors:\r\n" +
                string.Join("\r\n", removeResult.Errors));

        return removeResult;
    }

    #endregion

    #region Helper methods

    string GenerateImageFileName(int entityId, string originalFileName)
    {
        string initFileName = entityId.ToString() + ExtractFileExtention(originalFileName);
        return SanitizeFileName(initFileName);
    }

    protected string ExtractFileExtention(string fileName)
    {
        string ext = Path.GetExtension(fileName);
        return string.IsNullOrWhiteSpace(ext) ? ".jpg" : ext.ToLower();
    }

    string NormalizePath(string path)
    {
        return path.Replace('\\', Path.DirectorySeparatorChar).Replace('/', Path.DirectorySeparatorChar);
    }

    string SanitizeFileName(string fileName)
    {
        return Regex.Replace(fileName, @"[^a-zA-Z0-9_\.-]", "_");
    }

    string CombineMessages(params string[] messages)
    {
        StringBuilder sb = new();
        foreach (string message in messages) if (!string.IsNullOrWhiteSpace(message)) sb.AppendLine(message);
        if (sb.Length > 2) sb.Replace("\r\n", "", sb.Length - 2, 2);
        return sb.ToString();
    }

    #endregion

}
