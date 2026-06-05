using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Common; // Result modelini kullanabilmek için eklendi
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Users.Core.Features.Users.Commands.LogoutUser;

public record LogoutUserCommand(string AccessToken, string RefreshToken, bool LogoutAllDevices) : IRequest<Result<Unit>>
{
    public Guid UserId { get; set; }
}

public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Result<Unit>>
{
    private readonly IUsersDbContext _context;
    private readonly ITokenBlacklistService _blacklistService;
    private readonly ILocalizedMessageProvider _messageProvider; 
    public LogoutUserCommandHandler(
        IUsersDbContext context,
        ITokenBlacklistService blacklistService,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _blacklistService = blacklistService;
        _messageProvider = messageProvider;
    }

    public async Task<Result<Unit>> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            var userNotFoundMsg = _messageProvider.GetUserMessage("Users:UserNotFound");
            return Result<Unit>.Failure(userNotFoundMsg, HttpStatusCode.NotFound);
        }

        if (request.LogoutAllDevices)
        {
            user.RevokeAllRefreshTokens();
        }
        else
        {
            var currentToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
            if (currentToken != null)
            {
                user.RevokeSingleRefreshToken(currentToken);
            }
        }

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        if (handler.ReadToken(request.AccessToken) is System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken)
        {
            var remainingTime = jwtToken.ValidTo - DateTime.UtcNow;
            if (remainingTime > TimeSpan.Zero)
            {
                await _blacklistService.BlacklistTokenAsync(request.AccessToken, remainingTime);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}