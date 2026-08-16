using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Shelfly.Api.Data;
using Shelfly.Api.Services;

namespace Shelfly.Api.Tests.Unit.Cleanup;

public class CleanupServiceTests
{
    [Test]
    public async Task RestoreFromTrashAsync_ClearsDeletedAt()
    {
        // Arrange
        Guid bookId = Guid.NewGuid();
        var context = Substitute.For<ShelflyDbContext>();
        var logger = Substitute.For<ILogger<CleanupService>>();

        var existingBook = new Data.Entities.BookEntity
        {
            Id = bookId,
            Title = "Test Book",
            DeletedAt = DateTimeOffset.UtcNow.AddHours(-1)
        };

        context.Books
            .FirstOrDefaultAsync(Arg.Any<Expression<Func<Data.Entities.BookEntity, bool>>>())
            .Returns(Task.FromResult(existingBook));

        var service = new CleanupService(context, Substitute.For<IConfiguration>(), logger);

        // Act
        var result = await service.RestoreFromTrashAsync(bookId);

        // Assert
        result.ShouldNotBeNull();
        result.DeletedAt.ShouldBeNull();
        await context.Received().SaveChangesAsync();
    }

    [Test]
    public async Task HardDeleteExpiredItemsAsync_RemovesBooksPastRetention()
    {
        // Arrange
        var context = Substitute.For<ShelflyDbContext>();
        var logger = Substitute.For<ILogger<CleanupService>>();
        var config = Substitute.For<IConfiguration>();

        DateTimeOffset cutoffTime = DateTimeOffset.UtcNow.AddHours(-25);

        var expiredBook = new Data.Entities.BookEntity
        {
            Id = Guid.NewGuid(),
            Title = "Expired Book",
            DeletedAt = cutoffTime,
            Bookmarks = [new Data.Entities.BookmarkEntity { Id = Guid.NewGuid(), DeletedAt = cutoffTime }]
        };

        List<Data.Entities.BookEntity> expiredBooks = [expiredBook];
        IQueryable<Data.Entities.BookEntity> booksQuery = expiredBooks.AsQueryable();
        DbSet<Data.Entities.BookEntity> booksDbSet = Substitute.For<DbSet<Data.Entities.BookEntity>, IQueryable<Data.Entities.BookEntity>>();
        ((IQueryable<Data.Entities.BookEntity>)booksDbSet).Provider.Returns(booksQuery.Provider);
        ((IQueryable<Data.Entities.BookEntity>)booksDbSet).Expression.Returns(booksQuery.Expression);
        ((IQueryable<Data.Entities.BookEntity>)booksDbSet).ElementType.Returns(booksQuery.ElementType);

        context.Books.Returns(booksDbSet);

        IQueryable<Data.Entities.BookmarkEntity> bookmarksQuery = (expiredBook.Bookmarks).AsQueryable();
        DbSet<Data.Entities.BookmarkEntity> bookmarksDbSet = Substitute.For<DbSet<Data.Entities.BookmarkEntity>, IQueryable<Data.Entities.BookmarkEntity>>();
        ((IQueryable<Data.Entities.BookmarkEntity>)bookmarksDbSet).Provider.Returns(bookmarksQuery.Provider);
        ((IQueryable<Data.Entities.BookmarkEntity>)bookmarksDbSet).Expression.Returns(bookmarksQuery.Expression);
        ((IQueryable<Data.Entities.BookmarkEntity>)bookmarksDbSet).ElementType.Returns(bookmarksQuery.ElementType);

        context.Bookmarks.Returns(bookmarksDbSet);

        var service = new CleanupService(context, config, logger);

        // Act
        await service.HardDeleteExpiredItemsAsync();

        // Assert
        context.Received().Books.Remove(Arg.Any<Data.Entities.BookEntity>());
        context.Received().Bookmarks.RemoveRange(Arg.Any<IList<Data.Entities.BookmarkEntity>>());
        await context.Received().SaveChangesAsync();
    }
}
