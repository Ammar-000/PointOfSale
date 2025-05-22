using Helper.DataContainers;
using Helper.Interfaces;
using Microsoft.Extensions.Options;
using POS_Server_DAL.Settings;

namespace POS_Server_DAL.Repositories.Implementations;

public abstract class ImagesRepository
{
    protected readonly ICustomLogger _logger;
    protected readonly string _baseImagesPath;

    string LayerName => "DAL";

    public ImagesRepository(ICustomLogger logger, IOptions<ImagesSettings> imagesSettings)
    {
        _logger = logger;
        _baseImagesPath = imagesSettings.Value.BaseImagesPath;
    }

    #region Main methods

    public async Task<OperationResult<string>> GetImageUrlAsync(string imageSubPath)
    {
        string fullPath = Path.Combine(GetRootPath(), imageSubPath);
        if (!File.Exists(fullPath))
            return OperationResult<string>.Fail("Image not found.");

        string baseUrl = GetBaseUrl();
        string url = baseUrl + "/" + imageSubPath.Replace(Path.DirectorySeparatorChar, '/').TrimStart('/');
        return OperationResult<string>.Success(url);
    }

    public async Task<OperationResult<string>> AddImageAsync(Stream imageStream, string fileName, string subDirectory)
    {
        string failMsg = $"Failed to save image with name '{fileName}'.";

        OperationResult chkBaseImagesPathRes = ChkBaseImagesPath();
        if (!chkBaseImagesPathRes.Succeeded)
        {
            chkBaseImagesPathRes.AddError(failMsg);
            return OperationResult<string>.Fail(chkBaseImagesPathRes.Errors);
        }

        string folderPath = Path.Combine(GetRootPath(), _baseImagesPath, subDirectory);
        string fullPath = Path.Combine(folderPath, fileName);
        string imageSubPath = Path.Combine(_baseImagesPath, subDirectory, fileName)
            .Replace(Path.DirectorySeparatorChar, '/');

        try
        {
            Directory.CreateDirectory(folderPath);
            using FileStream fileStream = new(fullPath, FileMode.CreateNew, FileAccess.Write);
            await imageStream.CopyToAsync(fileStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, $"An error occurred while adding image with name '{fileName}'.");
            return OperationResult<string>.Fail(failMsg);
        }

        return OperationResult<string>.Success(imageSubPath);
    }

    public async Task<OperationResult<string>> UpdateImageAsync(string oldImageSubPath,
        Stream newImageStream, string newFileName, string subDirectory)
    {
        string failMsg = $"Failed to Update image with name {newFileName}.";

        OperationResult deleteResult = await DeleteImageAsync(oldImageSubPath);
        if (!deleteResult.Succeeded)
        {
            deleteResult.AddError(failMsg);
            return OperationResult<string>.Fail(deleteResult.Errors);
        }

        OperationResult<string> addResult = await AddImageAsync(newImageStream, newFileName, subDirectory);
        if (!addResult.Succeeded)
        {
            addResult.AddError(failMsg);
            return OperationResult<string>.Fail(addResult.Errors);
        }

        return addResult;
    }

    public async Task<OperationResult> DeleteImageAsync(string imageSubPath)
    {
        string fullPath = Path.Combine(GetRootPath(), imageSubPath);

        try
        {
            if (File.Exists(fullPath)) File.Delete(fullPath);
            else return OperationResult.Success($"Image with SubPath {imageSubPath} doesn't exist.");
        }
        catch (Exception ex)
        {
            _logger.LogError(LayerName, ex, $"An error occurred while deleting image with SubPath {imageSubPath}.");
            return OperationResult.Fail($"Failed to delete image with SubPath {imageSubPath}.");
        }

        return OperationResult.Success();
    }

    #endregion

    #region Helper methods

    protected OperationResult ChkBaseImagesPath()
    {
        if (string.IsNullOrWhiteSpace(_baseImagesPath)) return OperationResult.Fail("Base images path can't be empty.");
        return OperationResult.Success();
    }

    #endregion

    #region Abstract methods

    protected abstract string GetRootPath();
    protected abstract string GetBaseUrl();

    #endregion

}
