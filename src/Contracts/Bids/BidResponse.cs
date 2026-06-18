namespace BidPulse.Contracts.Bids;

public sealed record BidResponse(
    Guid Id,
    Guid LotId,
    Guid BidderId,
    decimal Amount,
    DateTimeOffset CreatedAt);