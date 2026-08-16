using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Data;

public class LocalDbContext(DbContextOptions<LocalDbContext> options) : DbContext(options)
{
    public DbSet<LocalBook> LocalBooks => Set<LocalBook>();
    public DbSet<LocalBookmark> LocalBookmarks => Set<LocalBookmark>();
    public DbSet<RemoteMapping> RemoteMappings => Set<RemoteMapping>();
    public DbSet<TrashConfigEntity> TrashConfigs => Set<TrashConfigEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // LocalBook indexes
        modelBuilder.Entity<LocalBook>()
            .HasIndex(b => b.LocalGuid)
            .IsUnique();

        modelBuilder.Entity<LocalBook>()
            .HasIndex(b => b.DeletionStatus);

        // LocalBookmark indexes
        modelBuilder.Entity<LocalBookmark>()
            .HasIndex(bm => bm.LocalGuid)
            .IsUnique();

        modelBuilder.Entity<LocalBookmark>()
            .HasIndex(bm => bm.LocalBookId);

        // RemoteMapping indexes
        modelBuilder.Entity<RemoteMapping>()
            .HasIndex(rm => new { rm.ServerUrl, rm.RemoteGuid });

        modelBuilder.Entity<RemoteMapping>()
            .HasIndex(rm => rm.LocalBookGuid);

        // Relationships
        modelBuilder.Entity<LocalBookmark>()
            .HasOne(bm => bm.LocalBook)
            .WithMany(b => b.LocalBookmarks)
            .HasForeignKey(bm => bm.LocalBookId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<RemoteMapping>()
            .HasOne(rm => rm.LocalBook)
            .WithMany(b => b.RemoteMappings)
            .HasForeignKey(rm => rm.LocalBookGuid)
            .OnDelete(DeleteBehavior.Cascade);

        // TrashConfig singleton
        modelBuilder.Entity<TrashConfigEntity>()
            .HasData(new TrashConfigEntity { Id = 1 });
    }
}
