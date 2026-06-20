using BidPulse.Contracts.AuctionLots;
using BidPulse.Database;
using BidPulse.Database.Entities;
using BidPulse.Database.Entities.Enums;
using BidPulse.Database.Entities.Models;
using BidPulse.Services.Abstractions;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Services.Implementations;

public sealed class AuctionLotService(
    AppDbContext context,
    ICurrentUserProvider currentUserProvider) : IAuctionLotService
{
    public async Task<ErrorOr<List<AuctionLotResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var lots = await context.Lots
            .AsNoTracking()
            .Include(l => l.Category)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(ct);
 
        return lots.Select(ToResponse).ToList();
    }
 
    public async Task<ErrorOr<AuctionLotResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var lot = await context.Lots
            .AsNoTracking()
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == id, ct);
 
        if (lot is null)
            return Error.NotFound("AuctionLot.NotFound", $"Auction lot with id '{id}' was not found.");
 
        return ToResponse(lot);
    }
 
    public async Task<ErrorOr<AuctionLotResponse>> CreateAsync(
        CreateAuctionLotRequest request, CancellationToken ct = default)
    {
        var creatorId = currentUserProvider.UserId;
        if (creatorId is null)
            return Error.Unauthorized("Auth.Unauthorized", "You must be authenticated to create an auction lot.");
 
        var categoryExists = await context.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
            return Error.NotFound("Category.NotFound", $"Category with id '{request.CategoryId}' was not found.");
 
        var createResult = AuctionLot.Create(new AuctionLotCreationParams(
            request.Title,
            request.Description,
            request.StartPrice,
            request.MinBidStep,
            request.StartsAt,
            request.EndsAt,
            request.CategoryId,
            creatorId.Value));
 
        if (createResult.IsError)
            return createResult.Errors;
 
        context.Lots.Add(createResult.Value);
        await context.SaveChangesAsync(ct);
 
        // Re-query to load the navigation property
        var created = await context.Lots
            .AsNoTracking()
            .Include(l => l.Category)
            .FirstAsync(l => l.Id == createResult.Value.Id, ct);
 
        return ToResponse(created);
    }
 
    public async Task<ErrorOr<AuctionLotResponse>> UpdateAsync(
        Guid id, UpdateAuctionLotRequest request, CancellationToken ct = default)
    {
        var currentUserId = currentUserProvider.UserId;
 
        var lot = await context.Lots
            .Include(l => l.Category)
            .FirstOrDefaultAsync(l => l.Id == id, ct);
 
        if (lot is null)
            return Error.NotFound("AuctionLot.NotFound", $"Auction lot with id '{id}' was not found.");
 
        if (currentUserId != lot.CreatorId)
            return Error.Forbidden(
                "AuctionLot.Forbidden",
                "You do not have permission to update this auction lot.");
 
        var categoryExists = await context.Categories.AnyAsync(c => c.Id == request.CategoryId, ct);
        if (!categoryExists)
            return Error.NotFound("Category.NotFound", $"Category with id '{request.CategoryId}' was not found.");
 
        var updateResult = lot.UpdateAuctionLot(new AuctionLotCreationParams(
            request.Title,
            request.Description,
            request.StartPrice,
            request.MinBidStep,
            request.StartsAt,
            request.EndsAt,
            request.CategoryId,
            lot.CreatorId));
 
        if (updateResult.IsError)
            return updateResult.Errors;
 
        await context.SaveChangesAsync(ct);
 
        // Re-query after save so Category nav prop reflects any CategoryId change
        var updated = await context.Lots
            .AsNoTracking()
            .Include(l => l.Category)
            .FirstAsync(l => l.Id == id, ct);
 
        return ToResponse(updated);
    }
 
    public async Task<ErrorOr<Deleted>> CancelAsync(Guid id, CancellationToken ct = default)
    {
        var currentUserId = currentUserProvider.UserId;
 
        var lot = await context.Lots.FindAsync([id], ct);
        if (lot is null)
            return Error.NotFound("AuctionLot.NotFound", $"Auction lot with id '{id}' was not found.");
 
        if (currentUserId != lot.CreatorId)
            return Error.Forbidden(
                "AuctionLot.Forbidden",
                "You do not have permission to cancel this auction lot.");
 
        var cancelResult = lot.Cancel();
        if (cancelResult.IsError)
            return cancelResult.Errors;
 
        // If there is a current top bidder their funds are still frozen — return them.
        var topBid = await context.Bids
            .Where(b => b.LotId == id)
            .OrderByDescending(b => b.Amount)
            .FirstOrDefaultAsync(ct);
 
        if (topBid is not null)
        {
            var topBidder = await context.Users.FindAsync([topBid.BidderId], ct);
            if (topBidder is not null)
            {
                var refundResult = WalletTransaction.Create(new WalletTransactionCreationParams(
                    topBidder.Id, topBid.Amount, TransactionType.Refund, lot.Id));
 
                if (refundResult.IsError)
                    return refundResult.Errors;
 
                var addResult = topBidder.AddToBalance(topBid.Amount);
                if (addResult.IsError)
                    return addResult.Errors;
 
                context.WalletTransactions.Add(refundResult.Value);
            }
        }
 
        await context.SaveChangesAsync(ct);
        return Result.Deleted;
    }
 
    private static AuctionLotResponse ToResponse(AuctionLot l) => new(
        l.Id,
        l.Title,
        l.Description,
        l.StartPrice,
        l.CurrentPrice,
        l.MinBidStep,
        l.StartsAt,
        l.EndsAt,
        l.Status.ToString(),
        l.CategoryId,
        l.Category.Name,
        l.CreatorId,
        l.CreatedAt);
}