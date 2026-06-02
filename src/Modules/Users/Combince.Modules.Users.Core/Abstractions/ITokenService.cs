using System.Security.Claims;
using Combince.Modules.Users.Core.Entities;

namespace Combince.Modules.Users.Core.Abstractions;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}