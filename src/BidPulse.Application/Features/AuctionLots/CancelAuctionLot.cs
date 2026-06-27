using BidPulse.Application.Common.Interfaces;
using ErrorOr;
using FluentValidation;
using Immediate.Handlers.Shared;

namespace BidPulse.Application.Features.AuctionLots;

[Handler]
public static partial class CancelAuctionLot
{
    public sealed record Command(Guid Id);

    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Id).NotEmpty().WithMessage("Id is required");
        }
    }

    private static async ValueTask<ErrorOr<Updated>> HandleAsync(
        Command request,
        IAppDbContext context,
        IValidator<Command> validator,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.Errors
                .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
                .ToList();

        var lot = await context.Lots.FindAsync([request.Id], ct);
        if (lot is null)
            return Error.NotFound("AuctionLot.NotFound", "Auction lot not found.");

        var result = lot.Cancel();
        if (result.IsError)
            return result.Errors;

        await context.SaveChangesAsync(ct);
        return Result.Updated;
    }
}