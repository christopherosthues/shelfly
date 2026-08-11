namespace Shelfly.Api.Data.Entities;

public class BookmarkEntity
{
    public Guid Id { get; set; }
    public int PageNumber { get; set; }
    public Guid BookId { get; set; }
    public BookEntity? Book { get; set; }
}
