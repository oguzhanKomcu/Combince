using Combince.Modules.Users.Core.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Users.Core.Features.Users.Commands.LoginUser;

public record LoginResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt);

public record LoginUserCommand(string EmailOrUsername, string Password) : IRequest<LoginResponse>;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.EmailOrUsername).NotEmpty().WithMessage("E-posta veya kullanıcı adı boş geçilemez.");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Şifre boş geçilemez.");
    }
}

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, LoginResponse>
{
    private readonly IUsersDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;

    public LoginUserCommandHandler(IUsersDbContext context, IPasswordHasher passwordHasher, IJwtTokenService tokenService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedInput = request.EmailOrUsername.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == normalizedInput || u.Username == normalizedInput, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            throw new InvalidOperationException("Geçersiz e-posta, kullanıcı adı veya şifre.");

        if (!user.IsActive)
            throw new InvalidOperationException("Bu kullanıcı hesabı askıya alınmış.");

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenString = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        user.AddRefreshToken(refreshTokenString, refreshTokenExpiresAt);
        await _context.SaveChangesAsync(cancellationToken); 

        return new LoginResponse(accessToken, refreshTokenString, refreshTokenExpiresAt);
    }
}