using Helper.DataContainers;
using POS_Domains.Models;

namespace POS_Server_BLL.Interfaces.BaseInterfaces;

public interface IImageService<T> where T : BaseModel
{
    public void InitImageServiceDelegates(Func<string> getSubDirectory, Func<int, Task<OperationResult<T>>> getEntityByIdAsync,
        Predicate<T> chkHasImage, Func<T, string?> getImageSubPath, Func<T, string, string, Task<OperationResult<T>>> saveImageSubPathAsync,
        Func<T, string, Task<OperationResult<T>>> removeImgSubPathAsync);
    public Task<OperationResult<string>> GetImageUrlAsync(int entityId);
    public Task<OperationResult<T>> AddImageAsync(int entityId, Stream imageStream, string originalFileName, string currUserId);
    public Task<OperationResult<T>> UpdateImageAsync(int entityId, Stream newImageStream, string originalNewFileName, string currUserId);
    public Task<OperationResult<T>> DeleteImageAsync(int entityId, string currUserId);
}
