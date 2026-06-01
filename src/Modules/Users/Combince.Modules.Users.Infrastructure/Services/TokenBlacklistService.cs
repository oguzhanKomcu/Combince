using Combince.Modules.Users.Core.Abstractions;
using Microsoft.Extensions.Caching.Memory;

namespace Combince.Modules.Users.Infrastructure.Services;

public class TokenBlacklistService : ITokenBlacklistService
{
    private readonly IMemoryCache _memoryCache;

    public TokenBlacklistService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task BlacklistTokenAsync(string token, TimeSpan expiryTime)
    {
        _memoryCache.Set(token, true, expiryTime);
        return Task.CompletedTask;
    }

    public Task<bool> IsTokenBlacklistedAsync(string token)
    {
        var isBlacklisted = _memoryCache.TryGetValue(token, out _);
        return Task.FromResult(isBlacklisted);
    }
}