namespace Shelfly.Common;

public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public string ISBN { get; set; }
    public DateTime PublishDate { get; set; }

    public List<Bookmark> Bookmarks { get; set; } = [];
}