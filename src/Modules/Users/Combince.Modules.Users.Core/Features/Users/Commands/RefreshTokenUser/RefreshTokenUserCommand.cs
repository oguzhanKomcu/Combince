using System.Net;
using System.Security.Claims;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Common;

namespace Combince.Modules.Users.Core.Features.Users.Commands.RefreshTokenUser;

public record RefreshTokenUserCommand(string ExpiredAccessToken, string RefreshToken) : IRequest<Result<TokenResponseDto>>;

public record TokenResponseDto(string AccessToken, string RefreshToken, DateTime AccessTokenExpiration);

public class RefreshTokenUserCommandHandler : IRequestHandler<RefreshTokenUserCommand, Result<TokenResponseDto>>
{
    private readonly IUsersDbContext _context;
    private readonly IJwtTokenService _tokenService;
    private readonly ILocalizedMessageProvider _messageProvider;

    public RefreshTokenUserCommandHandler(
        IUsersDbContext context,
        IJwtTokenService tokenService,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _tokenService = tokenService;
        _messageProvider = messageProvider;
    }

    public async Task<Result<TokenResponseDto>> Handle(RefreshTokenUserCommand request, CancellationToken cancellationToken)
    {
        var principal = _tokenService.GetPrincipalFromExpiredToken(request.ExpiredAccessToken);
        if (principal == null)
        {
            var invalidTokenMsg = _messageProvider.GetUserMessage("InvalidToken");
            return Result<TokenResponseDto>.Failure(invalidTokenMsg, HttpStatusCode.BadRequest);
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            var invalidUserClaimMsg = _messageProvider.GetUserMessage("InvalidUserClaim");
            return Result<TokenResponseDto>.Failure(invalidUserClaimMsg, HttpStatusCode.BadRequest);
        }

        var userId = Guid.Parse(userIdClaim);

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive, cancellationToken);

        if (user == null)
        {
            var userNotFoundMsg = _messageProvider.GetUserMessage("UserNotFound");
            return Result<TokenResponseDto>.Failure(userNotFoundMsg, HttpStatusCode.NotFound);
        }

        var savedRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);

        if (savedRefreshToken == null || savedRefreshToken.IsExpired || !savedRefreshToken.IsActive)
        {
            var tokenExpiredMsg = _messageProvider.GetUserMessage("RefreshTokenExpired");
            return Result<TokenResponseDto>.Failure(tokenExpiredMsg, HttpStatusCode.Unauthorized);
        }

        savedRefreshToken.Revoke();

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var rawRefreshTokenStr = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(7);

        user.AddRefreshToken(rawRefreshTokenStr, refreshTokenExpiry);

        await _context.SaveChangesAsync(cancellationToken);

        var response = new TokenResponseDto(newAccessToken, rawRefreshTokenStr, DateTime.UtcNow.AddMinutes(15));
        return Result<TokenResponseDto>.Success(response);
    }
}