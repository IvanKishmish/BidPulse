namespace BidPulse.Contracts.Bids;

public sealed record PlaceBidRequest(
    Guid LotId,
    decimal Amount);