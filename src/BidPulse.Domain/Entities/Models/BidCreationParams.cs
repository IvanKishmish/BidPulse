namespace BidPulse.Domain.Entities.Models;

public sealed record BidCreationParams(
    Guid LotId,
    Guid BidderId,
    decimal Amount
);