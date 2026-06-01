namespace Combince.Modules.Users.Core.Abstractions;

public interface ITokenBlacklistService
{
    Task BlacklistTokenAsync(string token, TimeSpan expiryTime);
    Task<bool> IsTokenBlacklistedAsync(string token);
}