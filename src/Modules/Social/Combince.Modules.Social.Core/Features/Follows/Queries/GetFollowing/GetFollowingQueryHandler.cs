using Combince.Modules.Social.Core.Abstractions;
using Combince.Modules.Social.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Combince.Modules.Social.Core.Features.Follows.Queries.GetFollowing;

public record GetFollowingQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<List<FollowingDto>>>;

public record FollowingDto(Guid UserId, string Username, string ProfilePictureUrl, DateTime FollowedAt);

public class GetFollowingQueryHandler : IRequestHandler<GetFollowingQuery, Result<List<FollowingDto>>>
{
    private readonly ISocialDbContext _context;

    public GetFollowingQueryHandler(ISocialDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FollowingDto>>> Handle(GetFollowingQuery request, CancellationToken cancellationToken)
    {
        var following = await _context.Follows
            .Where(follow => follow.FollowerId == request.UserId)
            .OrderByDescending(follow => follow.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(follow => new FollowingDto(
                follow.FollowingId,
                _context.SocialUsers
                    .Where(user => user.UserId == follow.FollowingId)
                    .Select(user => user.Username)
                    .FirstOrDefault() ?? "Bilinmeyen Kullanıcı",
                _context.SocialUsers
                    .Where(user => user.UserId == follow.FollowingId)
                    .Select(user => user.ProfilePictureUrl)
                    .FirstOrDefault() ?? string.Empty,
                follow.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<List<FollowingDto>>.Success(following);
    }
}