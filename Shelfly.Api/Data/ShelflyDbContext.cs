using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data.Entities;

namespace Shelfly.Api.Data;

public class ShelflyDbContext(DbContextOptions<ShelflyDbContext> options) : DbContext(options)
{
    public DbSet<BookEntity> Books => Set<BookEntity>();

    public DbSet<BookmarkEntity> Bookmarks => Set<BookmarkEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookEntity>().HasQueryFilter(b => b.DeletedAt == null);
    }
}
