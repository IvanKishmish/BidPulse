using BidPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Application.Common.Interfaces;

public interface IAppDbContext : IDisposable, IAsyncDisposable
{
    DbSet<User> Users { get; }
    DbSet<Category> Categories { get; }
    DbSet<AuctionLot> Lots { get; }
    DbSet<Bid> Bids { get; }
    DbSet<WalletTransaction> WalletTransactions { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}