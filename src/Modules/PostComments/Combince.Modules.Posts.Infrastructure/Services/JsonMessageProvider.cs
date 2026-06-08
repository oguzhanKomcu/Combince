using Combince.Modules.PostComments.Core.Abstractions;
using Microsoft.Extensions.Configuration;

namespace Combince.Modules.PostComments.Infrastructure.Services;

public class JsonMessageProvider : ILocalizedMessageProvider
{
    private readonly IConfiguration _configuration;

    public JsonMessageProvider(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetUserMessage(string key)
    {
        return GetMessage("UserMessages", key);
    }

    public string GetMessage(string group, string key)
    {
        var message = _configuration[$"{group}:{key}"];

        if (string.IsNullOrEmpty(message))
        {
            return $"[Key '{group}:{key}' not found in errors.json]";
        }

        return message;
    }
}