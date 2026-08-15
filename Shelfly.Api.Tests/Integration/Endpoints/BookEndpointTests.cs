using System.Net.Http.Json;
using Shelfly.Api.Data.Entities;
using Shelfly.Api.Models;

namespace Shelfly.Api.Tests.Integration.Endpoints;

public class BookEndpointTests : IntegrationTestBase
{
    [Test]
    public async Task GetBooks_ReturnsOnlyCurrentUserBooks()
    {
        // Arrange
        Guid userId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid userId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        await SeedDatabaseAsync(ctx =>
        {
            ctx.Books.AddRange(
                new BookEntity { Id = Guid.NewGuid(), Title = "User1 Book 1", Author = "Author 1", ISBN = "11111111111111", PublishDate = new DateTime(2023, 1, 1), UserId = userId1 },
                new BookEntity { Id = Guid.NewGuid(), Title = "User1 Book 2", Author = "Author 2", ISBN = "11111111111112", PublishDate = new DateTime(2023, 2, 1), UserId = userId1 },
                new BookEntity { Id = Guid.NewGuid(), Title = "User2 Book 1", Author = "Author 3", ISBN = "22222222222222", PublishDate = new DateTime(2023, 3, 1), UserId = userId2 }
            );
        });

        HttpClient httpClient = CreateHttpClient();
        await SetAuthorizationHeaderAsync(httpClient);

        // Act
        HttpResponseMessage response = await httpClient.GetAsync("/api/books");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("User1 Book 1");
        content.ShouldContain("User1 Book 2");
        content.ShouldNotContain("User2 Book 1");
    }

    [Test]
    public async Task GetBookById_ReturnsBookWithEmbeddedBookmarks()
    {
        // Arrange
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookId = Guid.NewGuid();

        await SeedDatabaseAsync(ctx =>
        {
            BookEntity book = new() { Id = bookId, Title = "Test Book", Author = "Author 1", ISBN = "12345678901234", PublishDate = new DateTime(2023, 1, 1), UserId = userId };
            ctx.Books.Add(book);

            ctx.Bookmarks.AddRange(
                new BookmarkEntity { Id = Guid.NewGuid(), StartPage = 1, EndPage = 5, Note = "Bookmark 1", UserId = userId, BookId = bookId },
                new BookmarkEntity { Id = Guid.NewGuid(), StartPage = 10, EndPage = 20, Note = "Bookmark 2", UserId = userId, BookId = bookId }
            );
        });

        HttpClient httpClient = CreateHttpClient();
        await SetAuthorizationHeaderAsync(httpClient);

        // Act
        HttpResponseMessage response = await httpClient.GetAsync($"/api/books/{bookId}");

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Test Book");
        content.ShouldContain("Bookmark 1");
        content.ShouldContain("Bookmark 2");
    }

    [Test]
    public async Task CreateBook_Returns201WithCorrectUserId()
    {
        // Arrange
        HttpClient httpClient = CreateHttpClient();
        await SetAuthorizationHeaderAsync(httpClient);

        CreateBookRequest request = new("New Book", "Author 1", "12345678901234", new DateTime(2023, 1, 1));

        // Act
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/books", request);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.Created);
        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("New Book");
    }

    [Test]
    public async Task UpdateBook_Returns200WithUpdatedData()
    {
        // Arrange
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookId = Guid.NewGuid();

        await SeedDatabaseAsync(ctx =>
        {
            ctx.Books.Add(new BookEntity { Id = bookId, Title = "Original", Author = "Author 1", ISBN = "12345678901234", PublishDate = new DateTime(2023, 1, 1), UserId = userId });
        });

        HttpClient httpClient = CreateHttpClient();
        await SetAuthorizationHeaderAsync(httpClient);

        UpdateBookRequest request = new("Updated Title", "Author 2", "12345678901234", new DateTime(2023, 1, 1));

        // Act
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/api/books/{bookId}", request);

        // Assert
        response.IsSuccessStatusCode.ShouldBeTrue();
        string content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Updated Title");
    }

    [Test]
    public async Task DeleteBook_CascadesToBookmarks()
    {
        // Arrange
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookId = Guid.NewGuid();

        await SeedDatabaseAsync(ctx =>
        {
            BookEntity book = new() { Id = bookId, Title = "Test Book", Author = "Author 1", ISBN = "12345678901234", PublishDate = new DateTime(2023, 1, 1), UserId = userId };
            ctx.Books.Add(book);

            ctx.Bookmarks.AddRange(
                new BookmarkEntity { Id = Guid.NewGuid(), StartPage = 1, EndPage = 5, Note = "Bookmark 1", UserId = userId, BookId = bookId },
                new BookmarkEntity { Id = Guid.NewGuid(), StartPage = 10, EndPage = 20, Note = "Bookmark 2", UserId = userId, BookId = bookId }
            );
        });

        HttpClient httpClient = CreateHttpClient();
        await SetAuthorizationHeaderAsync(httpClient);

        // Act - delete the book
        HttpResponseMessage response = await httpClient.DeleteAsync($"/api/books/{bookId}");

        // Assert - deletion successful
        response.IsSuccessStatusCode.ShouldBeTrue();

        // Verify bookmarks were cascade deleted
        HttpResponseMessage getResponse = await httpClient.GetAsync($"/api/bookmarks/{bookId}");
        getResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }
}
