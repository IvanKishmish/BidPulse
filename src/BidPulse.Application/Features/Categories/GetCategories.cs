using BidPulse.Application.Common.Dtos;
using BidPulse.Application.Common.Interfaces;
using Immediate.Handlers.Shared;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Application.Features.Categories;

[Handler]
public static partial class GetCategories
{
    public sealed record Query;

    private static async ValueTask<IReadOnlyList<CategoryResponseDto>> HandleAsync(
        Query _,
        IAppDbContext context,
        CancellationToken ct = default)
        => await context.Categories
            .AsNoTracking()
            .Select(c => new CategoryResponseDto(c.Id, c.Name, c.Description))
            .ToListAsync(ct);
}