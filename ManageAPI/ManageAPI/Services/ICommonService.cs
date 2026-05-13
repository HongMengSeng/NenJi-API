namespace ManageAPI.Services;

public interface ICommonService
{
    Task<string> UploadAsync(IFormFile file, CancellationToken cancellationToken = default);
}
