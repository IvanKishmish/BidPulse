using BidPulse.Application.Common.Interfaces;
using BidPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Persistence.Context;

public sealed class AppDbContext(DbContextOptions options) 
    : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<AuctionLot> Lots => Set<AuctionLot>();
    public DbSet<Bid> Bids => Set<Bid>();
    public DbSet<WalletTransaction> WalletTransactions => Set<WalletTransaction>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}