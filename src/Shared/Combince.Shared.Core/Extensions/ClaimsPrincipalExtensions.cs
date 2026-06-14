using System;
using System.Security.Claims;

namespace Combince.Shared.Core.Extensions; 
public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user));

        var userIdString = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdString))
        {
            userIdString = user.FindFirst("id")?.Value ?? user.FindFirst("sub")?.Value;
        }

        if (Guid.TryParse(userIdString, out var userId))
        {
            return userId;
        }

        throw new InvalidOperationException("Geçerli bir kullanıcı kimliği (UserId) token içinde bulunamadı.");
    }
}