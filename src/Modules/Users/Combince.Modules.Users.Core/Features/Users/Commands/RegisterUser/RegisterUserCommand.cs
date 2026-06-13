using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Common;
using Combince.Modules.Users.Core.Entities;
using Combince.Shared.Core.Events.Modules.User;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Combince.Modules.Users.Core.Features.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Username,
    string Password,
    string? FullName) : IRequest<Result<Guid>>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:EmailNotEmpty"))
            .EmailAddress().WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:InvalidEmailFormat"));

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:UsernameNotEmpty"))
            .MinimumLength(3).WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:UsernameMinLength"))
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:UsernameInvalidChars"));

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:PasswordNotEmpty"))
            .MinimumLength(6).WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:PasswordMinLength"));
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IUsersDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILocalizedMessageProvider _messageProvider;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IUserValidationService _validationService;

    public RegisterUserCommandHandler(
        IUsersDbContext context,
        IPasswordHasher passwordHasher,
        ILocalizedMessageProvider messageProvider,
        IPublishEndpoint publishEndpoint,
        IUserValidationService validationService) 
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _messageProvider = messageProvider;
        _publishEndpoint = publishEndpoint;
        _validationService = validationService;
    }

    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (!await _validationService.IsEmailAvailableAsync(request.Email, cancellationToken))
        {
            var emailExistsMsg = _messageProvider.GetUserMessage("Users:EmailAlreadyExists");
            return Result<Guid>.Failure(emailExistsMsg, HttpStatusCode.BadRequest);
        }

        if (!await _validationService.IsUsernameAvailableAsync(request.Username, cancellationToken))
        {
            var usernameExistsMsg = _messageProvider.GetUserMessage("Users:UsernameAlreadyTaken");
            return Result<Guid>.Failure(usernameExistsMsg, HttpStatusCode.BadRequest);
        }

        if (!_validationService.IsPasswordStrongEnough(request.Password, out var passwordError))
        {
            return Result<Guid>.Failure(passwordError!, HttpStatusCode.BadRequest);
        }

        var hashedPassword = _passwordHasher.HashPassword(request.Password);
        var userId = Guid.NewGuid();

        var user = new User(
            id: userId,
            email: request.Email.ToLowerScope(),
            username: request.Username.ToLowerScope(),
            passwordHash: hashedPassword,
            fullName: request.FullName?.Trim()
        );


        var finalProfilePicture = string.IsNullOrWhiteSpace(user.ProfilePictureUrl)
            ? "/assets/images/default-avatar.png"
            : user.ProfilePictureUrl;

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        await _publishEndpoint.Publish(new UserRegisteredIntegrationEvent(
            user.Id,
            user.Username,
            finalProfilePicture));

        return Result<Guid>.Success(user.Id);
    }
}

internal static class StringExtensions
{
    public static string ToLowerScope(this string value) => value.ToLowerInvariant();
}