using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Users.Core.Features.Users.Commands.UpdatePassword;

public record UpdatePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Result<Unit>>
{
    public Guid UserId { get; set; }
}

public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:CurrentPasswordNotEmpty"));

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:NewPasswordNotEmpty"))
            .MinimumLength(6).WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:PasswordMinLength"))
            .NotEqual(x => x.CurrentPassword).WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:NewPasswordCannotBeSame"));
    }
}

public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, Result<Unit>>
{
    private readonly IUsersDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILocalizedMessageProvider _messageProvider;

    public UpdatePasswordCommandHandler(
        IUsersDbContext context,
        IPasswordHasher passwordHasher,
        ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _messageProvider = messageProvider;
    }

    public async Task<Result<Unit>> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            var userNotFoundMsg = _messageProvider.GetUserMessage("Users:UserNotFound");
            return Result<Unit>.Failure(userNotFoundMsg, HttpStatusCode.NotFound);
        }

        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
        {
            var wrongPasswordMsg = _messageProvider.GetUserMessage("Users:CurrentPasswordWrong");
            return Result<Unit>.Failure(wrongPasswordMsg, HttpStatusCode.BadRequest);
        }

        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        user.UpdatePassword(newPasswordHash);
        user.RevokeAllRefreshTokens();

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}