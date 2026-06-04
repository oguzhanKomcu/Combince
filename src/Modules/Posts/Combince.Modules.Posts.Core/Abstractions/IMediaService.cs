using Microsoft.AspNetCore.Http;

namespace Combince.Modules.Posts.Core.Abstractions;

public interface IMediaService
{
    Task<string> UploadAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default);
}