namespace Combince.Modules.Users.Core.Events;

public record UserLoggedInIntegrationEvent
{
    public Guid UserId { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public DateTime LoggedInAt { get; init; }

    public UserLoggedInIntegrationEvent() { }

    public UserLoggedInIntegrationEvent(Guid userId, string username, string email)
    {
        UserId = userId;
        Username = username;
        Email = email;
        LoggedInAt = DateTime.UtcNow;
    }
}