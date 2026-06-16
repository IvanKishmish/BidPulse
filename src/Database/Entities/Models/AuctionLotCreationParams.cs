using BidPulse.Database.Entities.Enums;

namespace BidPulse.Database.Entities.Models;

public sealed record AuctionLotCreationParams(
    string Title,
    string Description,
    decimal StartPrice,
    decimal MinBidStep,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    Guid CategoryId,
    Guid CreatorId
    );