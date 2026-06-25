using BidPulse.Application.Common.Interfaces;
using BidPulse.Persistence.Context;

namespace BidPulse.Persistence.UOF;

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
    
    //transactions later
}