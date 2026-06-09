using System;

namespace Combince.Modules.Social.Core.Entities;

public class Follow
{
    public Guid Id { get; private set; }
    public Guid FollowerId { get; private set; } 
    public Guid FollowingId { get; private set; } 
    public DateTime CreatedAt { get; private set; }

    private Follow() { } 

    public Follow(Guid followerId, Guid followingId)
    {

        Id = Guid.NewGuid();
        FollowerId = followerId;
        FollowingId = followingId;
        CreatedAt = DateTime.UtcNow;
    }
}