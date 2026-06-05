using System.Net;
using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Common;
using Combince.Modules.Users.Core.Events;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Users.Core.Features.Users.Commands.LoginUser;

public record LoginResponse(string AccessToken, string RefreshToken, DateTime RefreshTokenExpiresAt);
public record LoginUserCommand(string EmailOrUsername, string Password) : IRequest<Result<LoginResponse>>;

public class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.EmailOrUsername)
            .NotEmpty()
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:EmailOrUsernameNotEmpty"));

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:PasswordNotEmpty"));
    }
}

public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<LoginResponse>>
{
    private readonly IUsersDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _tokenService;
    private readonly ILocalizedMessageProvider _messageProvider;
    private readonly IEventBus _eventBus;

    public LoginUserCommandHandler(
        IUsersDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService tokenService,
        ILocalizedMessageProvider messageProvider,
        IEventBus eventBus)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _messageProvider = messageProvider;
        _eventBus = eventBus;
    }

    public async Task<Result<LoginResponse>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var normalizedInput = request.EmailOrUsername.Trim().ToLowerInvariant();

        var user = await _context.Users
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == normalizedInput || u.Username == normalizedInput, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            var invalidCredsMsg = _messageProvider.GetUserMessage("Users:InvalidLoginCredentials");
            return Result<LoginResponse>.Failure(invalidCredsMsg, HttpStatusCode.BadRequest);
        }

        if (!user.IsActive)
        {
            var suspendedMsg = _messageProvider.GetUserMessage("Users:UserAccountSuspended");
            return Result<LoginResponse>.Failure(suspendedMsg, HttpStatusCode.BadRequest);
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenString = _tokenService.GenerateRefreshToken();
        var refreshTokenExpiresAt = DateTime.UtcNow.AddDays(7);

        user.AddRefreshToken(refreshTokenString, refreshTokenExpiresAt);

        await _context.SaveChangesAsync(cancellationToken);

        await _eventBus.PublishAsync(new UserLoggedInIntegrationEvent(
            user.Id,
            user.Username,
            user.Email
        ), cancellationToken);

        var response = new LoginResponse(accessToken, refreshTokenString, refreshTokenExpiresAt);
        return Result<LoginResponse>.Success(response);
    }
}