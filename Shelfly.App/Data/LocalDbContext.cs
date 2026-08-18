using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Data;

public partial class LocalDbContext : DbContext
{
    private static readonly string DatabasePath = Path.Combine(
        FileSystem.AppDataDirectory, "shelfly.db");

    public DbSet<BookEntity> Books => Set<BookEntity>();
    public DbSet<BookmarkEntity> Bookmarks => Set<BookmarkEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite($"Data Source={DatabasePath}");
    }

    public async Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default)
    {
        await Database.MigrateAsync(cancellationToken);
    }
}