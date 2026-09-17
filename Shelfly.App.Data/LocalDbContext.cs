using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Data;

public partial class LocalDbContext(DbContextOptions<LocalDbContext> options) : DbContext(options)
{
    public DbSet<BookEntity> Books => Set<BookEntity>();
    public DbSet<BookmarkEntity> Bookmarks => Set<BookmarkEntity>();
    public DbSet<ServerEntity> Servers => Set<ServerEntity>();
    public DbSet<SavedServerEntry> SavedServerEntries => Set<SavedServerEntry>();
    public DbSet<SyncState> SyncStates => Set<SyncState>();
    public DbSet<BookServerMapping> BookServerMappings => Set<BookServerMapping>();
    public DbSet<BookProfileSyncRecord> BookProfileSyncRecords => Set<BookProfileSyncRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookEntity>().HasQueryFilter(book => book.DeletedAt == null);
    }

    public async Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default)
    {
        await Database.MigrateAsync(cancellationToken);
    }
}