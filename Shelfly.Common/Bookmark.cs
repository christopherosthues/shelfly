namespace Shelfly.Common;

public class Bookmark(
    Guid id,
    Guid bookId,
    int startPage,
    DateTime createdAt)
{
    public Guid Id { get; } = id;
    public Guid BookId { get; } = bookId;
    public int StartPage { get; } = startPage;
    public int? EndPage { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; } = createdAt;
    public DateTime? LastModifiedAt { get; set; }
}
