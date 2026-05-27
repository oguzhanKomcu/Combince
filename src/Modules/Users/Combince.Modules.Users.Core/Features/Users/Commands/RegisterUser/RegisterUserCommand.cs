using Combince.Modules.Users.Core.Abstractions;
using Combince.Modules.Users.Core.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Users.Core.Features.Users.Commands.RegisterUser;

public record RegisterUserCommand(
    string Email,
    string Username,
    string Password,
    string? FullName) : IRequest<Guid>;

public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi boş geçilemez.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Kullanıcı adı boş geçilemez.")
            .MinimumLength(3).WithMessage("Kullanıcı adı en az 3 karakter olmalıdır.")
            .Matches(@"^[a-zA-Z0-9_]+$").WithMessage("Kullanıcı adı sadece harf, rakam ve alt çizgi içerebilir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş geçilemez.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.");
    }
}

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Guid>
{
    private readonly IUsersDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterUserCommandHandler(IUsersDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Guid> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var isEmailUnique = await _context.Users
            .AllAsync(u => u.Email != request.Email, cancellationToken);

        if (!isEmailUnique)
            throw new InvalidOperationException("Bu e-posta adresi zaten kullanımda.");

        var isUsernameUnique = await _context.Users
            .AllAsync(u => u.Username != request.Username, cancellationToken);

        if (!isUsernameUnique)
            throw new InvalidOperationException("Bu kullanıcı adı zaten alınmış.");

        var hashedPassword = _passwordHasher.HashPassword(request.Password);

        var userId = Guid.NewGuid();
        var user = new User(
            id: userId,
            email: request.Email.Trim().ToLowerScope(),
            username: request.Username.Trim().ToLowerScope(),
            passwordHash: hashedPassword,
            fullName: request.FullName?.Trim()
        );

        _context.Users.Add(user);
        await _context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}

// Küçük bir yardımcı metot (Lokal olarak burada kalabilir veya Shared altına taşınabilir)
internal static class StringExtensions
{
    public static string ToLowerScope(this string value) => value.ToLowerInvariant();
}