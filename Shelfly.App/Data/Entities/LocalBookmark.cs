namespace Shelfly.App.Data.Entities;

public class LocalBookmark : BaseEntity
{
    public int StartPage { get; set; }
    public int? EndPage { get; set; }
    public string? Note { get; set; }

    public Guid LocalBookId { get; set; }
    public LocalBook? LocalBook { get; set; }

    public string PageRange => EndPage.HasValue ? $"{StartPage}-{EndPage}" : StartPage.ToString();
}
