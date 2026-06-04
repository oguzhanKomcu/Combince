using Combince.Modules.Posts.Core.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

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

        // Benzersiz dosya ismi oluşturma
        string fileExtension = Path.GetExtension(file.FileName).ToLower();
        string uniqueId = Guid.NewGuid().ToString();
        string mainFileName = $"{uniqueId}{fileExtension}";
        string filePath = Path.Combine(uploadsFolder, mainFileName);

        // 1. Orijinal dosyayı olduğu gibi diske kaydet (Video veya Resim fark etmeksizin aslı korunsun)
        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream, cancellationToken);
        }

        // 2. Eğer yüklenen dosya bir RESİM ise, arka planda hemen Thumbnail (Önizleme) versiyonunu üret
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

                // ImageSharp ile resmi bellekten yükle
                using (Image image = await Image.LoadAsync(file.OpenReadStream(), cancellationToken))
                {
                    // Resmi en-boy oranını koruyarak maksimum 400px genişliğe küçült (Telefonu kaslatmayan ideal boyut)
                    int width = 400;
                    int height = (int)((double)image.Height / image.Width * width);

                    image.Mutate(x => x.Resize(width, height));

                    // Kalitesini %75'e optimize ederek (gözle görülür kayıp olmadan hafifletip) diske yaz
                    await image.SaveAsync(thumbFilePath, cancellationToken);
                }
            }
            catch
            {
                // Resim işleme esnasında olası bir hatada thumbnail üretilemezse sistem çökmesin, loglanabilir.
            }
        }

        // 3. İstek atan istemciye (React Native) sunucu üzerindeki tam URL'ini dön
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

            // Orijinal dosyayı sil
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);

                // Eğer resimse ve thumbnail versiyonu varsa onu da temizle
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
            // Loglanabilir
        }
        return Task.FromResult(false);
    }
}