using BidPulse.Application.Common.Dtos;
using BidPulse.Application.Common.Interfaces;
using FluentValidation;
using Immediate.Handlers.Shared;
using Microsoft.EntityFrameworkCore;
using ErrorOr;

namespace BidPulse.Application.Features.AuctionLots;

[Handler]
public static partial class GetAuctionLotById
{
    public sealed record Query(Guid Id);

    public sealed class Validator : AbstractValidator<Query>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        }
    }

    private static async ValueTask<ErrorOr<AuctionLotResponseDto>> HandleAsync(
        Query request,
        IAppDbContext context,
        IValidator<Query> validator,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.Errors
                .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
                .ToList();

        var lot = await context.Lots
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == request.Id, ct);

        if (lot is null)
            return Error.NotFound("AuctionLot.NotFound", "Auction lot not found.");

        return new AuctionLotResponseDto(
            lot.Id, lot.Title, lot.Description,
            lot.StartPrice, lot.CurrentPrice, lot.MinBidStep,
            lot.StartsAt, lot.EndsAt, lot.Status,
            lot.CategoryId, lot.CreatorId);
    }
}