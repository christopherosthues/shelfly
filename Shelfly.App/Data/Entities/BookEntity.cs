using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shelfly.App.Data.Entities;

public class BookEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public DateTime? PublishDate { get; set; }
    public DateTime? DeletedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public static void Configure(EntityTypeBuilder<BookEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Author)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.ISBN)
            .IsRequired()
            .HasIndex(e => e.ISBN, "IX_BookEntity_ISBN")
            .IsUnique();

        builder.Property(e => e.Publisher)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => new { e.Title, e.Author, e.Publisher })
            .HasDatabaseName("IX_BookEntity_Search");
    }
}
