using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data.Entities;

namespace Shelfly.Api.Data;

public class ShelflyDbContext(DbContextOptions<ShelflyDbContext> options) : DbContext(options)
{
    public DbSet<BookEntity> Books => Set<BookEntity>();
    public DbSet<BookmarkEntity> Bookmarks => Set<BookmarkEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Author).HasMaxLength(256);
            entity.Property(e => e.ISBN).HasMaxLength(16);
            entity.Property(e => e.UserId).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasMany(e => e.Bookmarks)
                .WithOne(e => e.Book)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<BookmarkEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartPage).IsRequired();
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.UserId).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.BookId });
        });
    }
}
