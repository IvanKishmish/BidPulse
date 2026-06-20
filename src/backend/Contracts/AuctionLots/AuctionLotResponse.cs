namespace BidPulse.Contracts.AuctionLots;

public sealed record AuctionLotResponse(
    Guid Id,
    string Title,
    string Description,
    decimal StartPrice,
    decimal CurrentPrice,
    decimal MinBidStep,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    string Status,
    Guid CategoryId,
    string CategoryName,
    Guid CreatorId,
    DateTimeOffset CreatedAt);