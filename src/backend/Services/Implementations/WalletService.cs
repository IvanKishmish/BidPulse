using BidPulse.Contracts.Wallet;
using BidPulse.Database;
using BidPulse.Database.Entities;
using BidPulse.Database.Entities.Enums;
using BidPulse.Database.Entities.Models;
using BidPulse.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using ErrorOr;

namespace BidPulse.Services.Implementations;

public sealed class WalletService(
    AppDbContext context,
    ICurrentUserProvider currentUserProvider) : IWalletService
{
    public async Task<ErrorOr<List<WalletTransactionResponse>>> GetTransactionHistoryAsync(
        CancellationToken ct = default)
    {
        var userId = currentUserProvider.UserId;
        if (userId is null)
            return Error.Unauthorized("Auth.Unauthorized", "You must be authenticated to view your transactions.");
 
        var transactions = await context.WalletTransactions
            .AsNoTracking()
            .Where(t => t.UserId == userId.Value)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(ct);
 
        return transactions.Select(ToResponse).ToList();
    }
 
    public async Task<ErrorOr<WalletTransactionResponse>> DepositAsync(
        DepositRequest request, CancellationToken ct = default)
    {
        var userId = currentUserProvider.UserId;
        if (userId is null)
            return Error.Unauthorized("Auth.Unauthorized", "You must be authenticated to deposit funds.");
 
        var user = await context.Users.FindAsync([userId.Value], ct);
        if (user is null)
            return Error.NotFound("User.NotFound", "Authenticated user was not found.");
 
        // Validate & record the transaction first — the entity itself enforces amount > 0.
        var txResult = WalletTransaction.Create(new WalletTransactionCreationParams(
            user.Id,
            request.Amount,
            TransactionType.Deposit));
 
        if (txResult.IsError)
            return txResult.Errors;
 
        var addResult = user.AddToBalance(request.Amount);
        if (addResult.IsError)
            return addResult.Errors;
 
        context.WalletTransactions.Add(txResult.Value);
        await context.SaveChangesAsync(ct);
 
        return ToResponse(txResult.Value);
    }
 
    public async Task<ErrorOr<WalletTransactionResponse>> WithdrawAsync(
        WithdrawRequest request, CancellationToken ct = default)
    {
        var userId = currentUserProvider.UserId;
        if (userId is null)
            return Error.Unauthorized("Auth.Unauthorized", "You must be authenticated to withdraw funds.");
 
        var user = await context.Users.FindAsync([userId.Value], ct);
        if (user is null)
            return Error.NotFound("User.NotFound", "Authenticated user was not found.");
 
        // DeductFromBalance validates both amount > 0 and sufficient balance.
        var deductResult = user.DeductFromBalance(request.Amount);
        if (deductResult.IsError)
            return deductResult.Errors;
 
        var txResult = WalletTransaction.Create(new WalletTransactionCreationParams(
            user.Id,
            request.Amount,
            TransactionType.Withdraw));
 
        if (txResult.IsError)
            return txResult.Errors;
 
        context.WalletTransactions.Add(txResult.Value);
        await context.SaveChangesAsync(ct);
 
        return ToResponse(txResult.Value);
    }
 
    private static WalletTransactionResponse ToResponse(WalletTransaction t) =>
        new(t.Id, t.UserId, t.Amount, t.Type.ToString(), t.LotId, t.CreatedAt);
}