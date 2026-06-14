namespace Combince.Modules.Social.Core.Entities;

public class SavedPost
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PostId { get; private set; }
    public DateTime SavedAt { get; private set; }

    private SavedPost() { }

    public SavedPost(Guid userId, Guid postId)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        PostId = postId;
        SavedAt = DateTime.UtcNow;
    }
}