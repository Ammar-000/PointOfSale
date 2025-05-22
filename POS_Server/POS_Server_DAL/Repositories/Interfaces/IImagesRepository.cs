using Helper.DataContainers;

namespace POS_Server_DAL.Repositories.Interfaces;

public interface IImagesRepository
{
    public Task<OperationResult<string>> GetImageUrlAsync(string imageSubPath);
    public Task<OperationResult<string>> AddImageAsync(Stream imageStream, string fileName, string subDirectory);
    public Task<OperationResult<string>> UpdateImageAsync(
        string oldImageSubPath, Stream newImageStream, string newFileName, string subDirectory);
    public Task<OperationResult> DeleteImageAsync(string imageSubPath);
}