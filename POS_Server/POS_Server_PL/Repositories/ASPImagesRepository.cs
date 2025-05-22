using Helper.Interfaces;
using Microsoft.Extensions.Options;
using POS_Server_DAL.Repositories.Implementations;
using POS_Server_DAL.Repositories.Interfaces;
using POS_Server_DAL.Settings;

namespace POS_Server_PL.Repositories;

public class ASPImagesRepository : ImagesRepository, IImagesRepository
{
    readonly IWebHostEnvironment _env;
    readonly IHttpContextAccessor _httpContextAccessor;

    public ASPImagesRepository(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor,
        ICustomLogger logger, IOptions<ImagesSettings> imagesSettings) : base(logger, imagesSettings)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override string GetRootPath()
    {
        return _env.WebRootPath;
    }

    protected override string GetBaseUrl()
    {
        HttpRequest? request = _httpContextAccessor.HttpContext?.Request;
        return request == null ? string.Empty : $"{request.Scheme}://{request.Host}";
    }

}
