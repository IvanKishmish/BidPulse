using BidPulse.Application.Common.Dtos;
using BidPulse.Application.Common.Interfaces;
using Immediate.Handlers.Shared;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Application.Features.AuctionLots;

[Handler]
public static partial class GetAuctionLots
{
    public sealed record Query;

    private static async ValueTask<IReadOnlyList<AuctionLotResponseDto>> HandleAsync(
        Query _,
        IAppDbContext context,
        CancellationToken ct = default)
        => await context.Lots
            .AsNoTracking()
            .Select(l => new AuctionLotResponseDto(
                l.Id, l.Title, l.Description,
                l.StartPrice, l.CurrentPrice, l.MinBidStep,
                l.StartsAt, l.EndsAt, l.Status,
                l.CategoryId, l.CreatorId))
            .ToListAsync(ct);
}