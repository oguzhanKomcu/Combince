using Combince.Modules.Users.Core.Abstractions;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace Combince.Modules.Users.Core.Features.Users.Commands.LogoutUser;
public record LogoutUserCommand(string AccessToken, string RefreshToken, bool LogoutAllDevices) : IRequest<Unit>
{
    public Guid UserId { get; set; }
}

// Handler sınıfının güncel hali:
public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Unit>
{
    private readonly IUsersDbContext _context;
    private readonly ITokenBlacklistService _blacklistService;

    public LogoutUserCommandHandler(IUsersDbContext context, ITokenBlacklistService blacklistService)
    {
        _context = context;
        _blacklistService = blacklistService;
    }

    public async Task<Unit> Handle(LogoutUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null) throw new InvalidOperationException("Kullanıcı bulunamadı.");

        // 1. Refresh Token'ları zaten yaptığımız gibi iptal ediyoruz
        if (request.LogoutAllDevices)
            user.RevokeAllRefreshTokens();
        else
        {
            var currentToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
            if (currentToken != null) user.RevokeSingleRefreshToken(currentToken);
        }

        // 2. KRİTİK ADIM: Mevcut Access Token'ı çözüp kalan ömrünü hesaplıyoruz ve kara listeye alıyoruz
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        if (handler.ReadToken(request.AccessToken) is System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken)
        {
            var remainingTime = jwtToken.ValidTo - DateTime.UtcNow;
            if (remainingTime > TimeSpan.Zero)
            {
                // Token'ı kalan süresi kadar (örn: 12 dakika) kara listeye fırlatıyoruz
                await _blacklistService.BlacklistTokenAsync(request.AccessToken, remainingTime);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}