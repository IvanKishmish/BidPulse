using BidPulse.Contracts.Bids;
using BidPulse.Database;
using BidPulse.Database.Entities;
using BidPulse.Database.Entities.Enums;
using BidPulse.Database.Entities.Models;
using BidPulse.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;

namespace BidPulse.Services.Implementations;

public sealed class BidService(
    AppDbContext context,
    ICurrentUserProvider currentUserProvider) : IBidService
{
    public async Task<ErrorOr<List<BidResponse>>> GetByLotIdAsync(Guid lotId, CancellationToken ct = default)
    {
        var lotExists = await context.Lots.AnyAsync(l => l.Id == lotId, ct);
        if (!lotExists)
            return Error.NotFound("AuctionLot.NotFound", $"Auction lot with id '{lotId}' was not found.");
 
        var bids = await context.Bids
            .AsNoTracking()
            .Where(b => b.LotId == lotId)
            .OrderByDescending(b => b.Amount)
            .ToListAsync(ct);
 
        return bids.Select(ToResponse).ToList();
    }
 
    public async Task<ErrorOr<List<BidResponse>>> GetByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var userExists = await context.Users.AnyAsync(u => u.Id == userId, ct);
        if (!userExists)
            return Error.NotFound("User.NotFound", $"User with id '{userId}' was not found.");
 
        var bids = await context.Bids
            .AsNoTracking()
            .Where(b => b.BidderId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(ct);
 
        return bids.Select(ToResponse).ToList();
    }
 
    public async Task<ErrorOr<BidResponse>> PlaceBidAsync(PlaceBidRequest request, CancellationToken ct = default)
    {
        var bidderId = currentUserProvider.UserId;
        if (bidderId is null)
            return Error.Unauthorized("Auth.Unauthorized", "You must be authenticated to place a bid.");
 
        await using var transaction = await context.Database.BeginTransactionAsync(ct);
 
        try
        {
            // ── Step 1: Load & validate the lot ───────────────────────────────
            var lot = await context.Lots.FindAsync([request.LotId], ct);
 
            if (lot is null)
                return Error.NotFound("AuctionLot.NotFound", $"Auction lot with id '{request.LotId}' was not found.");
 
            if (lot.Status != LotStatus.Active)
                return Error.Validation("Bid.LotNotActive", "Bids can only be placed on active auction lots.");
 
            if (lot.EndsAt <= DateTimeOffset.UtcNow)
                return Error.Validation("Bid.LotExpired", "The auction for this lot has already ended.");
 
            // ── Step 2: Bidder must not be the lot creator ─────────────────────
            if (lot.CreatorId == bidderId.Value)
                return Error.Validation("Bid.OwnLot", "You cannot place a bid on your own auction lot.");
 
            // ── Step 3: Amount must exceed CurrentPrice + MinBidStep ───────────
            var minimumBid = lot.CurrentPrice + lot.MinBidStep;
            if (request.Amount <= minimumBid)
                return Error.Validation(
                    "Bid.AmountTooLow",
                    $"Bid amount must exceed the current price plus the minimum bid step. " +
                    $"Minimum valid bid: {minimumBid + 0.01m:F2} (current: {lot.CurrentPrice:F2}, step: {lot.MinBidStep:F2}).");
 
            // ── Step 4: Load bidder and verify balance ─────────────────────────
            var bidder = await context.Users.FindAsync([bidderId.Value], ct);
            if (bidder is null)
                return Error.NotFound("User.NotFound", "Authenticated user was not found in the database.");
 
            if (bidder.Balance < request.Amount)
                return Error.Validation(
                    "Bid.InsufficientFunds",
                    $"Insufficient balance. Required: {request.Amount:F2}, available: {bidder.Balance:F2}.");
 
            // ── Step 5: Freeze bidder's funds (HoldForBid) ────────────────────
            var holdTxResult = WalletTransaction.Create(new WalletTransactionCreationParams(
                bidder.Id,
                request.Amount,
                TransactionType.HoldForBid,
                lot.Id));
 
            if (holdTxResult.IsError)
                return holdTxResult.Errors;
 
            var deductResult = bidder.DeductFromBalance(request.Amount);
            if (deductResult.IsError)
                return deductResult.Errors;
 
            context.WalletTransactions.Add(holdTxResult.Value);
 
            // ── Step 6: Refund the previous top bidder (if any) ───────────────
            var previousBid = await context.Bids
                .Where(b => b.LotId == lot.Id)
                .OrderByDescending(b => b.Amount)
                .FirstOrDefaultAsync(ct);
 
            if (previousBid is not null)
            {
                var previousBidder = await context.Users.FindAsync([previousBid.BidderId], ct);
 
                if (previousBidder is not null)
                {
                    var refundTxResult = WalletTransaction.Create(new WalletTransactionCreationParams(
                        previousBidder.Id,
                        previousBid.Amount,
                        TransactionType.Refund,
                        lot.Id));
 
                    if (refundTxResult.IsError)
                        return refundTxResult.Errors;
 
                    var addResult = previousBidder.AddToBalance(previousBid.Amount);
                    if (addResult.IsError)
                        return addResult.Errors;
 
                    context.WalletTransactions.Add(refundTxResult.Value);
                }
            }
 
            // ── Step 7: Advance the lot's current price ────────────────────────
            var priceResult = lot.SetCurrentPrice(request.Amount);
            if (priceResult.IsError)
                return priceResult.Errors;
 
            // ── Step 8: Persist the new bid ────────────────────────────────────
            var bidResult = Bid.Create(new BidCreationParams(lot.Id, bidderId.Value, request.Amount));
            if (bidResult.IsError)
                return bidResult.Errors;
 
            context.Bids.Add(bidResult.Value);
 
            // ── Step 9: Commit ─────────────────────────────────────────────────
            await context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
 
            return ToResponse(bidResult.Value);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }
 
    private static BidResponse ToResponse(Bid b) =>
        new(b.Id, b.LotId, b.BidderId, b.Amount, b.CreatedAt);
}