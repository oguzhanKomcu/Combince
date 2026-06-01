using System.Security.Claims;
using Combince.Modules.Users.Core.Abstractions;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Combince.Modules.Users.Core.Features.Users.Commands.UpdatePassword;

/// <summary>
/// Kullanıcının mevcut şifresini doğrulayarak yeni şifre belirlemesini sağlayan MediatR komut nesnesi.
/// Güvenlik gerekçesiyle aktif kullanıcının ID'si endpoint seviyesinde token'dan okunup bu nesneye basılacaktır.
/// </summary>
public record UpdatePasswordCommand(string CurrentPassword, string NewPassword) : IRequest<Unit>
{
    public Guid UserId { get; set; }
}

public class UpdatePasswordCommandValidator : AbstractValidator<UpdatePasswordCommand>
{
    public UpdatePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mevcut şifreniz boş geçilemez.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre boş geçilemez.")
            .MinimumLength(6).WithMessage("Yeni şifre en az 6 karakter olmalıdır.")
            .NotEqual(x => x.CurrentPassword).WithMessage("Yeni şifreniz mevcut şifreniz ile aynı olamaz.");
    }
}

public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, Unit>
{
    private readonly IUsersDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public UpdatePasswordCommandHandler(IUsersDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(UpdatePasswordCommand request, CancellationToken cancellationToken)
    {
        // 1. Veritabanından şifresini değiştirmek isteyen kullanıcıyı çekiyoruz
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
            throw new InvalidOperationException("Kullanıcı sistemde bulunamadı.");

        // 2. İstemciden gelen mevcut şifrenin doğruluğunu kontrol ediyoruz
        if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            throw new InvalidOperationException("Mevcut şifreniz hatalı.");

        // 3. Yeni şifreyi hasleyip entity üzerinde güncelliyoruz
        var newPasswordHash = _passwordHasher.HashPassword(request.NewPassword);

        // Entity içinde bu atamayı yapabilmek için birazdan User entity'sine metot ekleyeceğiz
        user.UpdatePassword(newPasswordHash);

        // 4. Güvenlik zinciri gereği şifre değiştiği an kullanıcının tüm açık oturumlarını (Refresh Token'larını) patlatıyoruz
        user.RevokeAllRefreshTokens();

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}