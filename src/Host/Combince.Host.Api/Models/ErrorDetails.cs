using System.Text.Json;

namespace Combince.Host.Api.Models;

public record ErrorDetails(int StatusCode, string Message)
{
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}