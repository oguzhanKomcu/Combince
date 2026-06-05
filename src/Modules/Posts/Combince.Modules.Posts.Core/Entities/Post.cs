namespace Combince.Modules.Posts.Core.Entities;

public class Post
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string MediaUrl { get; private set; } = string.Empty;
    public bool IsVideo { get; private set; }
    public List<string> Tags { get; private set; } = new();
    public double AverageRating { get; private set; }
    public int TotalVotes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Post() { } // EF Core için

    public Post(Guid userId, string description, string mediaUrl, bool isVideo, List<string> tags)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Description = description;
        MediaUrl = mediaUrl;
        IsVideo = isVideo;
        Tags = tags ?? new List<string>();
        AverageRating = 0.0;
        TotalVotes = 0;
        CreatedAt = DateTime.UtcNow;
        IsActive = true;
    }

    public void UpdateCounters(double averageRating, int totalVotes)
    {
        AverageRating = averageRating;
        TotalVotes = totalVotes;
    }

    public void UpdateDescription(string newDescription)
    {
        Description = newDescription;
    }
}