namespace BidPulse.Contracts.AuctionLots;

public sealed record UpdateAuctionLotRequest(
    string Title,
    string Description,
    decimal StartPrice,
    decimal MinBidStep,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    Guid CategoryId);