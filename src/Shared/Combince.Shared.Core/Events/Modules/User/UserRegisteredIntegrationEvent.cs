using System;
using System.Collections.Generic;
using System.Text;

namespace Combince.Shared.Core.Events.Modules.User
{
    public record UserRegisteredIntegrationEvent(
        Guid UserId,
        string Username,
        string ProfilePictureUrl);
}
