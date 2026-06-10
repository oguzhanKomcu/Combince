namespace Combince.Modules.Users.Core.Entities;

public class User
{
    // EF Core için boş constructor şarttır ama dışarıya kapatıyoruz (protected)
    protected User() { }

    public User(Guid id, string email, string username, string passwordHash, string? fullName)
    {
        Id = id;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Username = username ?? throw new ArgumentNullException(nameof(username));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        FullName = fullName;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Email { get; private set; }
    public string Username { get; private set; }
    public string PasswordHash { get; private set; }
    public string? FullName { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public string? Bio { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }
    public int FollowersCount { get; set; } = 0;
    public int FollowingCount { get; set; } = 0;
    private readonly List<UserRefreshToken> _refreshTokens = new();
    public IReadOnlyCollection<UserRefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();


    public void UpdateProfile(string? fullName, string? bio, string? profilePictureUrl)
    {
        FullName = fullName;
        Bio = bio;
        ProfilePictureUrl = profilePictureUrl;
    }

    public void AddRefreshToken(string token, DateTime expiresAt)
    {
        if (_refreshTokens.Any(t => t.Token == token))
            return;
        var newRefreshToken = new UserRefreshToken(Guid.Empty, this.Id, token, expiresAt);

        _refreshTokens.Add(newRefreshToken);
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens.Where(t => t.IsActive))
        {
            token.Revoke();
        }
    }

    public void RevokeSingleRefreshToken(UserRefreshToken token)
    {
        var targetToken = _refreshTokens.FirstOrDefault(t => t.Token == token.Token);
        if (targetToken != null && targetToken.IsActive)
        {
            targetToken.Revoke(); // Senin UserRefreshToken içindeki mevcut Revoke metodun
        }
    }
    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Şifre özeti boş olamaz.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }


}