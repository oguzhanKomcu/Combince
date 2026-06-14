using Combince.Modules.Social.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Combince.Modules.Social.Core.Abstractions;
using Combince.Shared.Core.Common;

namespace Combince.Modules.Social.Core.Features.SavedPosts.Queries.GetSavedPosts;

public record GetSavedPostsQuery(Guid UserId, int PageNumber = 1, int PageSize = 10) : IRequest<Result<PagedList<SavedPostResponse>>>;

public record SavedPostResponse(Guid Id, Guid PostId, DateTime SavedAt);

public class GetSavedPostsQueryHandler : IRequestHandler<GetSavedPostsQuery, Result<PagedList<SavedPostResponse>>>
{
    private readonly ISocialDbContext _context;

    public GetSavedPostsQueryHandler(ISocialDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedList<SavedPostResponse>>> Handle(GetSavedPostsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.SavedPosts
            .Where(x => x.UserId == request.UserId)
            .OrderByDescending(x => x.SavedAt)
            .Select(x => new SavedPostResponse(x.Id, x.PostId, x.SavedAt));

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var pagedList = new PagedList<SavedPostResponse>(items, totalCount, request.PageNumber, request.PageSize);

        return Result<PagedList<SavedPostResponse>>.Success(pagedList);
    }
}