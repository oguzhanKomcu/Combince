using Combince.Modules.Posts.Core.Abstractions;
using Combince.Modules.Posts.Core.Common; // Her modülün kendi bağımsız Result yapısı
using Combince.Modules.Posts.Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;

namespace Combince.Modules.Posts.Core.Features.Posts.Commands;

// İstemciye (React Native) hem orijinal hem de optimize edilmiş thumbnail linkini teslim ediyoruz
public record CreatePostResponse(Guid PostId, string MediaUrl, string ThumbnailUrl);

public record CreatePostCommand(
    Guid UserId,
    string Description,
    IFormFile File,
    List<string> Tags) : IRequest<Result<CreatePostResponse>>;

public class CreatePostCommandHandler : IRequestHandler<CreatePostCommand, Result<CreatePostResponse>>
{
    private readonly IPostsDbContext _context;
    private readonly IMediaService _mediaService;

    public CreatePostCommandHandler(IPostsDbContext context, IMediaService mediaService)
    {
        _context = context;
        _mediaService = mediaService;
    }

    public async Task<Result<CreatePostResponse>> Handle(CreatePostCommand request, CancellationToken cancellationToken)
    {
        if (request.File == null || request.File.Length == 0)
        {
            return Result<CreatePostResponse>.Failure("Dosya yüklenmesi zorunludur.", HttpStatusCode.BadRequest);
        }

        string contentType = request.File.ContentType?.ToLowerInvariant() ?? string.Empty;
        string fileName = request.File.FileName?.ToLowerInvariant() ?? string.Empty;

        bool isVideo = contentType.Contains("video") ||
                       fileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) ||
                       fileName.EndsWith(".mov", StringComparison.OrdinalIgnoreCase);

        string uploadedMediaUrl;
        try
        {
            uploadedMediaUrl = await _mediaService.UploadAsync(request.File, "posts", cancellationToken);
        }
        catch (Exception ex)
        {
            return Result<CreatePostResponse>.Failure($"Dosya yüklenirken hata oluştu: {ex.Message}", HttpStatusCode.InternalServerError);
        }

        var post = new Post(
            request.UserId,
            request.Description,
            uploadedMediaUrl,
            isVideo,
            request.Tags
        );

        _context.Posts.Add(post);
        await _context.SaveChangesAsync(cancellationToken);

        var mediaUrl = post.MediaUrl;
        string thumbnailUrl = mediaUrl;

        if (!post.IsVideo && !string.IsNullOrEmpty(mediaUrl))
        {
            string directory = mediaUrl.Substring(0, mediaUrl.LastIndexOf('/') + 1);
            string pureFileName = Path.GetFileNameWithoutExtension(mediaUrl);
            string ext = Path.GetExtension(mediaUrl);

            thumbnailUrl = $"{directory}{pureFileName}_thumb{ext}";
        }

        var response = new CreatePostResponse(post.Id, mediaUrl, thumbnailUrl);
        return Result<CreatePostResponse>.Success(response);
    }
}