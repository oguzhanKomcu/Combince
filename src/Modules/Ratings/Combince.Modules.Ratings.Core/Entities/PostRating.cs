using System;

namespace Combince.Modules.Ratings.Core.Entities;

public class PostRating
{
    public Guid Id { get; private set; }
    public Guid PostId { get; private set; }
    public Guid UserId { get; private set; }
    public int Score { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private PostRating() { }

    public PostRating(Guid postId, Guid userId, int score)
    {
        Id = Guid.NewGuid();
        PostId = postId;
        UserId = userId;
        Score = score;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateScore(int newScore)
    {
        Score = newScore;
    }
}