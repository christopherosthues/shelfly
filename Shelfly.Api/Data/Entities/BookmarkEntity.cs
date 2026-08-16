namespace Shelfly.Api.Data.Entities;

public class BookmarkEntity
{
    public Guid Id { get; set; }
    public int StartPage { get; set; }
    public int? EndPage { get; set; }
    public string? Note { get; set; }
    public Guid UserId { get; set; }
    public Guid BookId { get; set; }

    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SoftDeletedAt { get; set; }
    public DateTimeOffset? HardDeletedAt { get; set; }

    public BookEntity? Book { get; set; }
}
