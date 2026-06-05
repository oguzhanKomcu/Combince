using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Common;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Users.Core.Features.Users.Commands.UpdateProfile;

public record UpdateProfileCommand(string? FullName, string? Bio, string? ProfilePictureUrl) : IRequest<Result<Unit>>
{
    public Guid UserId { get; set; }
}

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator(ILocalizedMessageProvider messageProvider)
    {
        RuleFor(x => x.FullName)
            .MaximumLength(100)
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:FullNameMaxLength"));

        RuleFor(x => x.Bio)
            .MaximumLength(500)
            .WithMessage(messageProvider.GetMessage("ValidationMessages", "Users:BioMaxLength"));
    }
}

public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<Unit>>
{
    private readonly IUsersDbContext _context;
    private readonly ILocalizedMessageProvider _messageProvider;

    public UpdateProfileCommandHandler(IUsersDbContext context, ILocalizedMessageProvider messageProvider)
    {
        _context = context;
        _messageProvider = messageProvider;
    }

    public async Task<Result<Unit>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            var errorMessage = _messageProvider.GetUserMessage("Users:UserNotFound");
            return Result<Unit>.Failure(errorMessage, HttpStatusCode.NotFound);
        }

        user.UpdateProfile(request.FullName, request.Bio, request.ProfilePictureUrl);

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success(Unit.Value);
    }
}