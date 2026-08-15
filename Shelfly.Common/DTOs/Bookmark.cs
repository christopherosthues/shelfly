namespace Shelfly.Common.DTOs;

public class Bookmark
{
    public Guid Id { get; set; }
    public int StartPage { get; set; }
    public int? EndPage { get; set; }
    public string? Note { get; set; }

    public string PageRange => EndPage.HasValue ? $"{StartPage}-{EndPage}" : StartPage.ToString();
}
