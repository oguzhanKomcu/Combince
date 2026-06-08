using System;

namespace Combince.Modules.PostComments.Core.Entities;

public class Comment
{
    public Guid Id { get; private set; }
    public Guid PostId { get; private set; }
    public Guid UserId { get; private set; }
    public string Content { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    private Comment() { } 

    public Comment(Guid postId, Guid userId, string content)
    {
        Id = Guid.NewGuid();
        PostId = postId;
        UserId = userId;
        Content = content;
        CreatedAt = DateTime.UtcNow;
    }

    public bool UpdateContent(string newContent)
    {
        if (string.IsNullOrWhiteSpace(newContent))
        {
            return false;
        }

        Content = newContent;
        UpdatedAt = DateTime.UtcNow;

        return true;
    }
}