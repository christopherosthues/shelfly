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
            entity.HasIndex(e => e.DeletionStatus);  // Index for trash filtering queries
            entity.HasMany(e => e.Bookmarks)
                .WithOne(e => e.Book)
                .HasForeignKey(e => e.BookId)
                .OnDelete(DeleteBehavior.Cascade);  // Cascade delete on hard deletion
        });

        modelBuilder.Entity<BookmarkEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StartPage).IsRequired();
            entity.Property(e => e.Note).HasMaxLength(1000);
            entity.Property(e => e.UserId).IsRequired();
            entity.HasIndex(e => new { e.UserId, e.BookId });
            entity.HasIndex(e => e.DeletionStatus);  // Index for trash filtering queries
            entity.HasIndex(e => e.BookId);          // Index for cascade delete joins
        });
    }
}
