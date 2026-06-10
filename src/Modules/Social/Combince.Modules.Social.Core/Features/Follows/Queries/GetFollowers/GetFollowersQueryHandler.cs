using Combince.Modules.Social.Core.Abstractions;
using Combince.Modules.Social.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Combince.Modules.Social.Core.Features.Follows.Queries.GetFollowers;

public record GetFollowersQuery(
    Guid UserId,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<List<FollowerDto>>>;

public record FollowerDto(Guid UserId, string Username, string ProfilePictureUrl, DateTime FollowedAt);

public class GetFollowersQueryHandler : IRequestHandler<GetFollowersQuery, Result<List<FollowerDto>>>
{
    private readonly ISocialDbContext _context;

    public GetFollowersQueryHandler(ISocialDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<FollowerDto>>> Handle(GetFollowersQuery request, CancellationToken cancellationToken)
    {
        var followers = await _context.Follows
            .Where(follow => follow.FollowingId == request.UserId)
            .OrderByDescending(follow => follow.CreatedAt)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(follow => new FollowerDto(
                follow.FollowerId,
                _context.SocialUsers
                    .Where(user => user.UserId == follow.FollowerId)
                    .Select(user => user.Username)
                    .FirstOrDefault() ?? "Bilinmeyen Kullanıcı",
                _context.SocialUsers
                    .Where(user => user.UserId == follow.FollowerId)
                    .Select(user => user.ProfilePictureUrl)
                    .FirstOrDefault() ?? string.Empty,
                follow.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return Result<List<FollowerDto>>.Success(followers);
    }
}