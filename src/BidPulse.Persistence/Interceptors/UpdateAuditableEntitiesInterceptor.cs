using BidPulse.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BidPulse.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor for automatically auditing and logging entity creation and modification timestamps.
/// </summary>
/// <remarks>
/// It offloads infrastructure-related 
/// logic (cross-cutting concerns) from the <see cref="DbContext"/> class, keeping the database context purely 
/// declarative, clean, and compliant with the Single Responsibility Principle.
/// </remarks>
public sealed class UpdateAuditableEntitiesInterceptor : SaveChangesInterceptor
{
    
    /// <summary>
    /// Asynchronously intercepts the change-saving process before SQL commands are executed against the database.
    /// </summary>
    /// <param name="eventData">Contextual data regarding the EF Core event, containing a reference to the current <see cref="DbContext"/>.</param>
    /// <param name="result">The current result of the operation, which can be modified or overridden by the interceptor.</param>
    /// <param name="cancellationToken">The cancellation token to observe while waiting for the task to complete.</param>
    /// <returns>The result of the save operation, wrapped within a <see cref="ValueTask"/>.</returns>
    /// <remarks>
    /// Overriding <see cref="SavingChangesAsync"/> (rather than its synchronous counterpart) is critical, 
    /// as modern backend applications operate fully asynchronously. We modify the entity state directly in memory 
    /// (within the ChangeTracker) before EF Core generates and sends the corresponding <c>INSERT</c> or <c>UPDATE</c> SQL statements.
    /// </remarks>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync
    (DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
       DbContext? context = eventData.Context;

       if (context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

       var entries = context.ChangeTracker.Entries<Entity>();
       var utcNow = DateTimeOffset.UtcNow;
       
       foreach (var entry in entries)
       {
           if (entry.State == EntityState.Added)
           {
               entry.Property(x => x.CreatedAt).CurrentValue = utcNow;
           }

           if (entry.State == EntityState.Modified)
           {
               entry.Property(x => x.UpdatedAt).CurrentValue = utcNow;
           }
       }

       return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}