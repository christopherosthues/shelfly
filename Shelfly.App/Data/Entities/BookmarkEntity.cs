using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Shelfly.App.Data.Entities;

public class BookmarkEntity
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public int StartPage { get; set; }
    public int? EndPage { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }

    public BookEntity? Book { get; set; }

    public static void Configure(EntityTypeBuilder<BookmarkEntity> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BookId)
            .IsRequired()
            .HasIndex(e => e.BookId, "IX_BookmarkEntity_BookId");

        builder.Property(e => e.StartPage)
            .IsRequired()
            .CheckConstraint("CK_BookmarkEntity_StartPage_Positive", "StartPage > 0");

        builder.Property(e => e.EndPage)
            .CheckConstraint("CK_BookmarkEntity_EndPage_GreaterOrEqual", "EndPage IS NULL OR EndPage >= StartPage");

        builder.Property(e => e.Note)
            .HasMaxLength(1000);

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        builder.HasIndex(e => new { e.BookId, e.StartPage })
            .HasDatabaseName("IX_BookmarkEntity_BookId_StartPage");

        builder.HasOne(e => e.Book)
            .WithMany()
            .HasForeignKey(e => e.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
