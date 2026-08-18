namespace Shelfly.Common;

public class Book(
    Guid id,
    string title,
    string author,
    string isbn,
    string publisher,
    DateTime? publishDate,
    DateTime createdAt)
{
    public Guid Id { get; } = id;
    public string Title { get; } = title;
    public string Author { get; } = author;
    public string ISBN { get; } = isbn;
    public string Publisher { get; } = publisher;
    public DateTime? PublishDate { get; } = publishDate;
    public DateTime CreatedAt { get; } = createdAt;
    public DateTime? LastModifiedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
