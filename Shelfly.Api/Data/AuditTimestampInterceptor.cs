using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shelfly.Api.Data.Entities;

namespace Shelfly.Api.Data;

public sealed class AuditTimestampInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = new())
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    private static void UpdateAuditableEntities(DbContext context)
    {
        DateTime utcNow = DateTime.UtcNow;

        foreach (EntityEntry<BookEntity> entry in context.ChangeTracker.Entries<BookEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedAt = utcNow;
            }
        }

        foreach (EntityEntry<BookmarkEntity> entry in context.ChangeTracker.Entries<BookmarkEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.LastModifiedAt = utcNow;
            }
        }
    }
}
