using System;
using System.Collections.Generic;
using System.Text;

namespace Combince.Shared.Core.Events.Modules.Follow
{
    public record UserUnfollowedIntegrationEvent(Guid FollowerId, Guid FollowingId, DateTime UnfollowedAt);
}
