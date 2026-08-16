namespace Shelfly.Api.Data.Entities;

public class BookEntity
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public DateTime PublishDate { get; set; }
    public Guid UserId { get; set; }

    public DateTimeOffset LastModified { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? SoftDeletedAt { get; set; }
    public DateTimeOffset? HardDeletedAt { get; set; }

    public ICollection<BookmarkEntity> Bookmarks { get; set; } = new List<BookmarkEntity>();
}
