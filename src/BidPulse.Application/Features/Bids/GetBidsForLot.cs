using BidPulse.Application.Common.Dtos;
using BidPulse.Application.Common.Interfaces;
using FluentValidation;
using Immediate.Handlers.Shared;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Application.Features.Bids;

[Handler]
public static partial class GetBidsForLot
{
    public sealed record Query(Guid LotId);

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.LotId).NotEmpty().WithMessage("LotId is required");
        }
    }

    private static async ValueTask<IReadOnlyList<BidResponseDto>> HandleAsync(
        Query request,
        IAppDbContext context,
        IValidator<Query> validator,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return [];

        return await context.Bids
            .AsNoTracking()
            .Where(b => b.LotId == request.LotId)
            .Select(b => new BidResponseDto(b.Id, b.LotId, b.BidderId, b.Amount))
            .ToListAsync(ct);
    }
}