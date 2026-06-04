using Combince.Modules.Posts.Core.Abstractions;
using Combince.Modules.Posts.Core.Common;
using Combince.Modules.Posts.Core.Features.Queries.GetPosts;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Combince.Modules.Posts.Core.Features.Posts.Queries.GetPosts;

public class GetPostsQueryHandler : IRequestHandler<GetPostsQuery, Result<PagedList<PostDto>>>
{
    private readonly IPostsDbContext _context;

    public GetPostsQueryHandler(IPostsDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedList<PostDto>>> Handle(GetPostsQuery request, CancellationToken cancellationToken)
    {
        // Sayfa parametrelerinin güvenliğini sağlıyoruz
        int page = request.Page <= 0 ? 1 : request.Page;
        int pageSize = request.PageSize <= 0 ? 10 : request.PageSize;

        // Toplam post sayısını hafif bir sorgu ile alıyoruz
        int totalCount = await _context.Posts.CountAsync(cancellationToken);

        // Veritabanından veriyi çekerken performansı uçurmak için AsNoTracking() kullanıyoruz
        var posts = await _context.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt) // En yeni kombinler en üstte
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        // Dto dönüşümü ve Thumbnail URL'lerinin dinamik üretimi
        var postDtos = posts.Select(post =>
        {
            string mediaUrl = post.MediaUrl;
            string thumbnailUrl = mediaUrl;

            // Video değilse ImageSharp ile sıkıştırdığımız _thumb takılı resmi üret
            if (!post.IsVideo && !string.IsNullOrEmpty(mediaUrl))
            {
                string directory = mediaUrl.Substring(0, mediaUrl.LastIndexOf('/') + 1);
                string pureFileName = Path.GetFileNameWithoutExtension(mediaUrl);
                string ext = Path.GetExtension(mediaUrl);
                thumbnailUrl = $"{directory}{pureFileName}_thumb{ext}";
            }

            return new PostDto(
                post.Id,
                post.UserId,
                post.Description,
                mediaUrl,
                thumbnailUrl,
                post.IsVideo,
                post.Tags,
                post.AverageRating,
                post.TotalVotes,
                post.CreatedAt
            );
        }).ToList();

        var pagedResult = new PagedList<PostDto>(postDtos, page, pageSize, totalCount);
        return Result<PagedList<PostDto>>.Success(pagedResult);
    }
}