using System.Net;
using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Users.Core.Features.Users.Queries.GetProfileByUserId;

public record GetProfileByUserIdQuery(Guid UserId) : IRequest<Result<UserProfileDto>>;

public record UserProfileDto(
    Guid Id,
    string Email,
    string Username,
    string? FullName,
    string? Bio,
    string? ProfilePictureUrl,
    DateTime CreatedAt);


public class GetProfileByUserIdQueryHandler : IRequestHandler<GetProfileByUserIdQuery, Result<UserProfileDto>>
{
    private readonly IUsersDbContext _context;
    private readonly ILocalizedMessageProvider _messageProvider;

    public GetProfileByUserIdQueryHandler(IUsersDbContext context, ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _messageProvider = messageProvider;
    }

    public async Task<Result<UserProfileDto>> Handle(GetProfileByUserIdQuery request, CancellationToken cancellationToken)
    {
        var userProfile = await _context.Users
            .Where(u => u.Id == request.UserId && u.IsActive)
            .Select(u => new UserProfileDto(
                u.Id,
                u.Email,
                u.Username,
                u.FullName,
                u.Bio,
                u.ProfilePictureUrl,
                u.CreatedAt
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (userProfile == null)
        {
            var errorMessage = _messageProvider.GetUserMessage("UserNotFoundOrPassive");

            return Result<UserProfileDto>.Failure(errorMessage, HttpStatusCode.NotFound);
        }

        return Result<UserProfileDto>.Success(userProfile);
    }
}