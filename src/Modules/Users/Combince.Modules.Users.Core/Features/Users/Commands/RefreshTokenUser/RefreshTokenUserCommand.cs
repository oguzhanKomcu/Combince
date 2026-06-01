using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Features.Users.Commands.LoginUser;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Combince.Modules.Users.Core.Features.Users.Commands.RefreshTokenUser;

/// <summary>
/// İstemciden gelen token yenileme parametrelerini sarmalayan MediatR komut nesnesi.
/// Giriş işleminde olduğu gibi LoginResponse record yapısını geri döndürür.
/// </summary>
public record RefreshTokenUserCommand(string AccessToken, string RefreshToken) : IRequest<LoginResponse>;

/// <summary>
/// RefreshTokenUserCommand için FluentValidation kurallarını barındıran doğrulama sınıfı.
/// </summary>
public class RefreshTokenUserCommandValidator : AbstractValidator<RefreshTokenUserCommand>
{
    public RefreshTokenUserCommandValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Süresi dolmuş erişim anahtarı (Access Token) boş geçilemez.");

        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Yenileme anahtarı (Refresh Token) boş geçilemez.");
    }
}


/// <summary>
/// Süresi dolmuş erişim anahtarını ve yenileme anahtarını doğrulayarak yeni token paketi üreten komut işleyici sınıfı.
/// </summary>
public class RefreshTokenUserCommandHandler : IRequestHandler<RefreshTokenUserCommand, LoginResponse>
{
    private readonly IUsersDbContext _context;
    private readonly IJwtTokenService _tokenService;
    private readonly IConfiguration _configuration;

    public RefreshTokenUserCommandHandler(
        IUsersDbContext context,
        IJwtTokenService tokenService,
        IConfiguration configuration)
    {
        _context = context;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<LoginResponse> Handle(RefreshTokenUserCommand request, CancellationToken cancellationToken)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal == null)
            throw new InvalidOperationException("Geçersiz erişim anahtarı (Access Token).");

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new InvalidOperationException("Erişim anahtarı içerisinde geçerli bir kullanıcı kimliği bulunamadı.");

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException("Kullanıcı sistemde bulunamadı.");

        if (!user.IsActive)
            throw new InvalidOperationException("Bu kullanıcı hesabı askıya alınmış.");

        var existingToken = user.RefreshTokens
            .FirstOrDefault(t => t.Token == request.RefreshToken);

        if (existingToken == null)
            throw new InvalidOperationException("Geçersiz yenileme anahtarı (Refresh Token).");

        if (!existingToken.IsActive || existingToken.ExpiresAt <= DateTime.UtcNow)
            throw new InvalidOperationException("Yenileme anahtarının süresi dolmuş veya iptal edilmiş. Lütfen tekrar giriş yapın.");

        user.RevokeAllRefreshTokens();

        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenString = _tokenService.GenerateRefreshToken();
        var newRefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        user.AddRefreshToken(newRefreshTokenString, newRefreshTokenExpiresAt);
        await _context.SaveChangesAsync(cancellationToken);

        return new LoginResponse(newAccessToken, newRefreshTokenString, newRefreshTokenExpiresAt);
    }

    /// <summary>
    /// Süresi dolmuş token'ın imzasını doğrulayarak içindeki kullanıcı bilgilerini söker.
    /// </summary>
    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
            ValidateLifetime = false // Süresi bittiği için validasyonun patlamasını engelliyoruz
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Geçersiz imza algoritması.");

        return principal;
    }
}