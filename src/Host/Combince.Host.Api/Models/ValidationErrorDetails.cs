using System.Text.Json;

namespace Combince.Host.Api.Models;

public record ValidationErrorDetails(int StatusCode, string Message, IDictionary<string, string[]> Errors)
{
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}