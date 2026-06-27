namespace BidPulse.Application.Common.Dtos;

public sealed record BidResponseDto(
    Guid Id,
    Guid LotId,
    Guid BidderId,
    decimal Amount);