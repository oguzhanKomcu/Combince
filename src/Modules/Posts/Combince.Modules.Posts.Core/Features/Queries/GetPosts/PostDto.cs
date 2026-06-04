using Combince.Modules.Posts.Core.Common;
using MediatR;

namespace Combince.Modules.Posts.Core.Features.Queries.GetPosts;

public record PostDto(
    Guid Id,
    Guid UserId,
    string Description,
    string MediaUrl,
    string ThumbnailUrl,
    bool IsVideo,
    List<string> Tags,
    double AverageRating,
    int TotalVotes,
    DateTime CreatedAt);

public record GetPostsQuery(int Page = 1, int PageSize = 10) : IRequest<Result<PagedList<PostDto>>>;

public class PagedList<T>
{
    public List<T> Items { get; }
    public int Page { get; }
    public int PageSize { get; }
    public int TotalCount { get; }
    public bool HasNextPage => Page * PageSize < TotalCount;

    public PagedList(List<T> items, int page, int pageSize, int totalCount)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalCount = totalCount;
    }
}