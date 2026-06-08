using System;

namespace Combince.Shared.Core.Events.Modules.PostRating;

public record PostRatingUpdatedIntegrationEvent(
    Guid PostId,
    double AverageRating,
    int TotalVotes);