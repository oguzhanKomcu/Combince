using System;

namespace Combince.Modules.Social.Core.Entities;

public class SocialUser
{
    public Guid UserId { get; private set; } 
    public string Username { get; private set; } = string.Empty;
    public string ProfilePictureUrl { get; private set; } = string.Empty;

    private SocialUser() { }

    public SocialUser(Guid userId, string username, string profilePictureUrl)
    {
        UserId = userId;
        Username = username;
        ProfilePictureUrl = profilePictureUrl;
    }

    public void UpdateProfile(string username, string profilePictureUrl)
    {
        Username = username;
        ProfilePictureUrl = profilePictureUrl;
    }
}