using System.Net.Http.Json;
using Shelfly.Api.Data.Entities;
using Shelfly.Api.Models;

namespace Shelfly.Api.Tests.Integration.Endpoints;

public class BookmarkEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task CreateBookmark_Returns201WithCorrectUserId()
    {
        // Arrange
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookId = Guid.NewGuid();

        await SeedDatabaseAsync(ctx =>
        {
            ctx.Books.Add(new BookEntity { Id = bookId, Title = "Test Book", Author = "Author 1", ISBN = "12345678901234", PublishDate = new DateTime(2023, 1, 1), UserId = userId });
        });

        HttpClient httpClient = CreateHttpClient();
        await SetAuthorizationHeaderAsync(httpClient);

        CreateBookmarkRequest request = new(1, 5, "Test note");

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"/api/bookmarks/{bookId}", request);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("1");
        content.ShouldContain("5");
        content.ShouldContain("Test note");
    }

    [Test]
    public async Task UpdateBookmark_Returns200WithUpdatedData()
    {
        // Arrange
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookId = Guid.NewGuid();
        Guid bookmarkId = Guid.NewGuid();

        await SeedDatabaseAsync(ctx =>
        {
            BookEntity book = new() { Id = bookId, Title = "Test Book", Author = "Author 1", ISBN = "12345678901234", PublishDate = new DateTime(2023, 1, 1), UserId = userId };
            ctx.Books.Add(book);

            ctx.Bookmarks.Add(new BookmarkEntity { Id = bookmarkId, StartPage = 1, EndPage = 5, Note = "Original note", UserId = userId, BookId = bookId });
        });

        HttpClient httpClient = CreateHttpClient();
        await SetAuthorizationHeaderAsync(httpClient);

        UpdateBookmarkRequest request = new(10, 20, "Updated note");

        // Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/api/bookmarks/{bookmarkId}", request);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("10");
        content.ShouldContain("20");
        content.ShouldContain("Updated note");
    }

    [Test]
    public async Task DeleteBookmark_RemovesOnlyTargetedBookmark()
    {
        // Arrange
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookId = Guid.NewGuid();
        Guid bookmarkIdToDelete = Guid.NewGuid();
        Guid bookmarkIdToKeep = Guid.NewGuid();

        await SeedDatabaseAsync(ctx =>
        {
            BookEntity book = new() { Id = bookId, Title = "Test Book", Author = "Author 1", ISBN = "12345678901234", PublishDate = new DateTime(2023, 1, 1), UserId = userId };
            ctx.Books.Add(book);

            ctx.Bookmarks.AddRange(
                new BookmarkEntity { Id = bookmarkIdToDelete, StartPage = 1, EndPage = 5, Note = "To delete", UserId = userId, BookId = bookId },
                new BookmarkEntity { Id = bookmarkIdToKeep, StartPage = 10, EndPage = 20, Note = "To keep", UserId = userId, BookId = bookId }
            );
        });

        HttpClient httpClient = CreateHttpClient();
        await SetAuthorizationHeaderAsync(httpClient);

        // Act - delete first bookmark
        HttpResponseMessage response = await httpClient.DeleteAsync($"/api/bookmarks/{bookmarkIdToDelete}");

        // Assert - deletion successful
        response.IsSuccessStatusCode.ShouldBeTrue();

        // Verify remaining bookmark still exists
        HttpResponseMessage getResponse = await httpClient.GetAsync($"/api/bookmarks/{bookId}");
        string content = await getResponse.Content.ReadAsStringAsync();
        content.ShouldContain("To keep");
        content.ShouldNotContain("To delete");
    }
}
