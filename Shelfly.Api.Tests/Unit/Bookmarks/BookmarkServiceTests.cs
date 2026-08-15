using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shelfly.Api.Bookmarks;
using Shelfly.Api.Data;
using Shelfly.Api.Data.Entities;
using Shelfly.Common.DTOs;

namespace Shelfly.Api.Tests.Unit.Bookmarks;

public class BookmarkServiceTests
{
    [Test]
    public async Task GetBookmarksAsync_ReturnsOnlyMatchingUserIdAndBookId()
    {
        // Arrange
        Guid userId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");

        List<BookmarkEntity> entities = [
            new() { Id = Guid.NewGuid(), StartPage = 1, EndPage = 5, Note = "Note 1", UserId = userId1, BookId = bookId1 },
            new() { Id = Guid.NewGuid(), StartPage = 10, EndPage = 20, Note = "Note 2", UserId = userId1, BookId = bookId1 },
            new() { Id = Guid.NewGuid(), StartPage = 30, EndPage = 40, Note = "Note 3", UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"), BookId = bookId1 },
            new() { Id = Guid.NewGuid(), StartPage = 50, EndPage = 60, Note = "Note 4", UserId = userId1, BookId = Guid.Parse("00000000-0000-0000-0000-000000000002") }
        ];

        IQueryable<BookmarkEntity> queryable = entities.AsQueryable();

        DbSet<BookmarkEntity> bookmarksDbSet = Substitute.For<DbSet<BookmarkEntity>, IQueryable<BookmarkEntity>>();
        ((IQueryable<BookmarkEntity>)bookmarksDbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<BookmarkEntity>)bookmarksDbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<BookmarkEntity>)bookmarksDbSet).ElementType.Returns(queryable.ElementType);

        ShelflyDbContext context = Substitute.For<ShelflyDbContext>(Arg.Any<DbContextOptions<ShelflyDbContext>>());
        context.Bookmarks.Returns(bookmarksDbSet);

        BookmarkService service = new(context);

        // Act
        List<Bookmark> result = await service.GetBookmarksAsync(userId1, bookId1);

        // Assert
        result.Count.ShouldBe(2);
        result.All(b => b.Note == "Note 1" || b.Note == "Note 2").ShouldBeTrue();
    }

    [Test]
    public async Task GetBookmarkAsync_ReturnsNullForNonMatchingUserId()
    {
        // Arrange
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookmarkId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        List<BookmarkEntity> entities = [
            new() { Id = bookmarkId, StartPage = 1, EndPage = 5, Note = "Note 1", UserId = Guid.Parse("00000000-0000-0000-0000-000000000002"), BookId = Guid.NewGuid() }
        ];

        IQueryable<BookmarkEntity> queryable = entities.AsQueryable();

        DbSet<BookmarkEntity> bookmarksDbSet = Substitute.For<DbSet<BookmarkEntity>, IQueryable<BookmarkEntity>>();
        ((IQueryable<BookmarkEntity>)bookmarksDbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<BookmarkEntity>)bookmarksDbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<BookmarkEntity>)bookmarksDbSet).ElementType.Returns(queryable.ElementType);

        ShelflyDbContext context = Substitute.For<ShelflyDbContext>(Arg.Any<DbContextOptions<ShelflyDbContext>>());
        context.Bookmarks.Returns(bookmarksDbSet);

        BookmarkService service = new(context);

        // Act
        Bookmark? result = await service.GetBookmarkAsync(userId, bookmarkId);

        // Assert
        result.ShouldBeNull();
    }
}
