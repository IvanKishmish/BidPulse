using BidPulse.Domain.Enums;

namespace BidPulse.Application.Common.Dtos;

public sealed record AuctionLotResponseDto(
    Guid Id,
    string Title,
    string Description,
    decimal StartPrice,
    decimal CurrentPrice,
    decimal MinBidStep,
    DateTimeOffset StartsAt,
    DateTimeOffset EndsAt,
    LotStatus Status,
    Guid CategoryId,
    Guid CreatorId);