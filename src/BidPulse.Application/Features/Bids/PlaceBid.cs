using BidPulse.Application.Common.Interfaces;
using BidPulse.Domain.Entities;
using BidPulse.Domain.Entities.Models;
using BidPulse.Domain.Enums;
using FluentValidation;
using Immediate.Handlers.Shared;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Application.Features.Bids;

[Handler]
public static partial class PlaceBid
{
    public sealed record Command(Guid LotId, Guid BidderId, decimal Amount);
    
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.LotId).NotEmpty().WithMessage("LotId is required");
            RuleFor(x => x.BidderId).NotEmpty().WithMessage("BidderId is required");
            RuleFor(x => x.Amount).GreaterThan(0).WithMessage("Amount must be greater than zero");
        }
    }

    private static async ValueTask<ErrorOr<Guid>> HandleAsync(
        Command request,
        IAppDbContext context,
        IUnitOfWork uof,
        IValidator<Command> validator,
        CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            return validationResult.Errors
                .Select(e => Error.Validation(e.PropertyName, e.ErrorMessage))
                .ToList();

        var lot = await context.Lots.FindAsync([request.LotId], ct);

        if (lot is null)
            return Error.NotFound("AuctionLot.NotFound", "Auction lot not found.");
        
        if(lot.Status != LotStatus.Active)
            return Error.Validation("AuctionLot.NotActive", "Auction lot is not active.");
        
        if(request.Amount < lot.CurrentPrice + lot.MinBidStep)
            return Error.Validation(
                "Bid.TooLow",
                $"Bid must be at least {lot.CurrentPrice + lot.MinBidStep}.");
        
        var bidder = await context.Users.FindAsync([request.BidderId], ct);
        
        if (bidder is null)
            return Error.NotFound("User.NotFound", "Bidder not found.");

        if (bidder.Balance < request.Amount)
            return Error.Validation("User.InsufficientFunds", "Insufficient balance.");
        
        var previousBid = await context.Bids
            .AsNoTracking()
            .Where(b => b.LotId == request.LotId)
            .OrderByDescending(b => b.BidderId)
            .FirstOrDefaultAsync(ct);

        await uof.BeginTransactionAsync(ct);

        try
        {
            if (previousBid is not null)
            {
                var previousBidder = await context.Users.FindAsync([previousBid.BidderId], ct);

                if (previousBidder is not null)
                {
                    var refundResult = previousBidder.AddToBalance(previousBid.Amount);
                    if (refundResult.IsError)
                        return refundResult.Errors;

                    var refundTransaction = WalletTransaction.Create(new WalletTransactionCreationParams(
                        previousBidder.Id,
                        previousBid.Amount,
                        TransactionType.Refund,
                        lot.Id));

                    if (refundTransaction.IsError)
                        return refundTransaction.Errors;

                    await context.WalletTransactions.AddAsync(refundTransaction.Value, ct);
                }
            }

            var deductResult = bidder.DeductFromBalance(request.Amount);

            if (deductResult.IsError)
                return deductResult.Errors;

            var holdTransaction = WalletTransaction.Create(new WalletTransactionCreationParams(
                bidder.Id,
                request.Amount,
                TransactionType.HoldForBid,
                lot.Id));

            if (holdTransaction.IsError)
                return holdTransaction.Errors;

            await context.WalletTransactions.AddAsync(holdTransaction.Value, ct);

            var bidResult = Bid.Create(new BidCreationParams(
                request.LotId,
                request.BidderId,
                request.Amount));

            if (bidResult.IsError)
                return bidResult.Errors;

            await context.Bids.AddAsync(bidResult.Value, ct);

            var priceResult = lot.SetCurrentPrice(request.Amount);
            if (priceResult.IsError)
                return priceResult.Errors;

            await uof.SaveChangesAsync(ct);
            await uof.CommitAsync(ct);

            return bidResult.Value.Id;
        }
        catch
        {
            await uof.RollbackAsync(ct);
            return Error.Failure("PlaceBid.Failed", "An error occurred while placing the bid.");
        }
    }
}