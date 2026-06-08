using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.Posts.Core.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace Combince.Modules.Posts.Infrastructure.Services;

public class LocalMediaService : IMediaService
{
    private readonly IWebHostEnvironment _env;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public LocalMediaService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
    {
        _env = env;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> UploadAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default)
    {
        string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        string uploadsFolder = Path.Combine(webRoot, "uploads", folderName);

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string fileExtension = Path.GetExtension(file.FileName).ToLower();
        string uniqueId = Guid.NewGuid().ToString();
        string mainFileName = $"{uniqueId}{fileExtension}";
        string filePath = Path.Combine(uploadsFolder, mainFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        bool isImage = file.ContentType.Contains("image") ||
                        fileExtension == ".jpg" ||
                        fileExtension == ".jpeg" ||
                        fileExtension == ".png" ||
                        fileExtension == ".webp";

        if (isImage)
        {
            try
            {
                string thumbFileName = $"{uniqueId}_thumb{fileExtension}";
                string thumbFilePath = Path.Combine(uploadsFolder, thumbFileName);

                using var inputStream = file.OpenReadStream();
                using var managedStream = new SKManagedStream(inputStream);
                using var originalBitmap = SKBitmap.Decode(managedStream);

                if (originalBitmap != null)
                {
                    int width = 400;
                    int height = (int)((double)originalBitmap.Height / originalBitmap.Width * width);

                    using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.Medium);
                    if (resizedBitmap != null)
                    {
                        using var image = SKImage.FromBitmap(resizedBitmap);
                        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 75);
                        using var thumbStream = new FileStream(thumbFilePath, FileMode.Create);

                        data.SaveTo(thumbStream);
                    }
                }
            }
            catch
            {
            }
        }

        var request = _httpContextAccessor.HttpContext?.Request;
        var baseUrl = $"{request?.Scheme}://{request?.Host}";

        return $"{baseUrl}/uploads/{folderName}/{mainFileName}";
    }

    public Task<bool> DeleteAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(fileUrl)) return Task.FromResult(false);

        try
        {
            var uri = new Uri(fileUrl);
            string webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            string fullPath = Path.Combine(webRoot, uri.LocalPath.TrimStart('/'));

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);

                string directory = Path.GetDirectoryName(fullPath) ?? string.Empty;
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fullPath);
                string ext = Path.GetExtension(fullPath);
                string thumbPath = Path.Combine(directory, $"{fileNameWithoutExt}_thumb{ext}");

                if (File.Exists(thumbPath))
                {
                    File.Delete(thumbPath);
                }

                return Task.FromResult(true);
            }
        }
        catch
        {
        }
        return Task.FromResult(false);
    }
}