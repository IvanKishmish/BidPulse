using BidPulse.Database.Entities;
using BidPulse.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace BidPulse.Database;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options, ICurrentUserProvider currentUserProvider)
    : DbContext(options)
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
    
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Entity>();
        var currentUserId = currentUserProvider.UserId;

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(x => x.CreatedAt).CurrentValue = DateTimeOffset.UtcNow;
                
                if (currentUserId.HasValue)
                {
                    entry.Property(x => x.CreatedBy).CurrentValue = currentUserId.Value;
                }
            }
            
            if (entry.State == EntityState.Modified)
            {
                entry.Property(x => x.UpdatedAt).CurrentValue = DateTimeOffset.UtcNow;
                
                if (currentUserId.HasValue)
                {
                    entry.Property(x => x.CreatedBy).CurrentValue = currentUserId.Value;
                }
            }
        }
        
        return base.SaveChangesAsync(cancellationToken);
    }
}