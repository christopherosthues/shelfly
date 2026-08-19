using System.Text.Json;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Library.Services;
using Shelfly.Common;

namespace Shelfly.App.Services;

public class LibraryExportService(LibraryService libraryService)
{
    public async Task<Result<string>> ExportLibraryToJsonAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            List<BookEntity> books = await libraryService.GetAllBooksAsync(cancellationToken);
            List<LibraryBookDto> exportData = [];

            foreach (BookEntity book in books)
            {
                List<BookmarkEntity> bookmarks = await libraryService.GetBookmarksByBookIdAsync(book.Id, cancellationToken);

                LibraryBookDto bookDto = new()
                {
                    Id = book.Id,
                    Title = book.Title,
                    Author = book.Author,
                    ISBN = book.ISBN,
                    Publisher = book.Publisher,
                    PublishDate = book.PublishDate,
                    CreatedAt = book.CreatedAt,
                    LastModifiedAt = book.LastModifiedAt,
                    Bookmarks =
                    [
                        .. bookmarks.Select(b => new LibraryBookmarkDto
                        {
                            Id = b.Id,
                            StartPage = b.StartPage,
                            EndPage = b.EndPage,
                            Note = b.Note,
                            CreatedAt = b.CreatedAt,
                            LastModifiedAt = b.LastModifiedAt
                        })
                    ]
                };

                exportData.Add(bookDto);
            }

            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingDefault
            };

            string json = JsonSerializer.Serialize(exportData, options);

            return Result<string>.Success(json);
        }
        catch (Exception ex)
        {
            return Result<string>.Failure($"Export failed: {ex.Message}");
        }
    }
}

public class LibraryBookDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ISBN { get; set; } = string.Empty;
    public string Publisher { get; set; } = string.Empty;
    public DateTime? PublishDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public List<LibraryBookmarkDto> Bookmarks { get; set; } = [];
}

public class LibraryBookmarkDto
{
    public Guid Id { get; set; }
    public int StartPage { get; set; }
    public int? EndPage { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
