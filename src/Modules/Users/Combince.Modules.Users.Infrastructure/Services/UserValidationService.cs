using Combince.Modules.Users.Core.Abstractions;
using Combince.Shared.Core.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Combince.Modules.Users.Infrastructure.Services;

public class UserValidationService : IUserValidationService
{
    private readonly IUsersDbContext _context;

    public UserValidationService(IUsersDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsEmailAvailableAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerScope();
        return !await _context.Users.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> IsUsernameAvailableAsync(string username, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = username.ToLowerScope();

        var blacklistedNames = new[] { "admin", "moderator", "combince", "root" };
        if (blacklistedNames.Contains(normalizedUsername)) return false;

        return !await _context.Users.AnyAsync(u => u.Username == normalizedUsername, cancellationToken);
    }

    public bool IsPasswordStrongEnough(string password, out string? errorMessage)
    {
        errorMessage = null;

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            errorMessage = "Şifre en az 6 karakter olmalıdır.";
            return false;
        }

        // İsteğe bağlı regex kuralları (En az bir büyük, bir küçük, bir sayı)
        if (!Regex.IsMatch(password, @"[A-Z]") || !Regex.IsMatch(password, @"[a-z]") || !Regex.IsMatch(password, @"[0-9]"))
        {
            errorMessage = "Şifre en az bir büyük harf, bir küçük harf ve bir rakam içermelidir.";
            return false;
        }

        return true;
    }
}