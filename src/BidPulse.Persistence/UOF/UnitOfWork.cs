using BidPulse.Application.Common.Interfaces;
using BidPulse.Persistence.Context;
using Microsoft.EntityFrameworkCore.Storage;

namespace BidPulse.Persistence.UOF;

public sealed class UnitOfWork(AppDbContext db) : IUnitOfWork
{
    private IDbContextTransaction? _transaction;
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await db.SaveChangesAsync(ct);
    
    public async Task BeginTransactionAsync(CancellationToken ct = default)
        => _transaction = await db.Database.BeginTransactionAsync(ct);
    
    public async Task CommitAsync(CancellationToken ct = default)
        => await _transaction!.CommitAsync(ct);

    public async Task RollbackAsync(CancellationToken ct = default)
        => await _transaction!.RollbackAsync(ct);
}