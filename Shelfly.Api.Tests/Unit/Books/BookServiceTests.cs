using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Shelfly.Api.Books;
using Shelfly.Api.Data;
using Shelfly.Api.Data.Entities;
using Shelfly.Common.DTOs;

namespace Shelfly.Api.Tests.Unit.Books;

public class BookServiceTests
{
    [Test]
    public async Task GetBooksAsync_ReturnsOnlyMatchingUserId()
    {
        // Arrange
        Guid userId1 = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid userId2 = Guid.Parse("00000000-0000-0000-0000-000000000002");

        List<BookEntity> entities = [
            new() { Id = Guid.NewGuid(), Title = "Book 1", Author = "Author 1", ISBN = "12345678901234", PublishDate = DateTime.Now, UserId = userId1 },
            new() { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author 2", ISBN = "12345678901235", PublishDate = DateTime.Now, UserId = userId1 },
            new() { Id = Guid.NewGuid(), Title = "Book 3", Author = "Author 3", ISBN = "12345678901236", PublishDate = DateTime.Now, UserId = userId2 }
        ];

        IQueryable<BookEntity> queryable = entities.AsQueryable();

        DbSet<BookEntity> booksDbSet = Substitute.For<DbSet<BookEntity>, IQueryable<BookEntity>>();
        ((IQueryable<BookEntity>)booksDbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<BookEntity>)booksDbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<BookEntity>)booksDbSet).ElementType.Returns(queryable.ElementType);

        ShelflyDbContext context = Substitute.For<ShelflyDbContext>(Arg.Any<DbContextOptions<ShelflyDbContext>>());
        context.Books.Returns(booksDbSet);

        BookService service = new(context);

        // Act
        List<Book> result = await service.GetBooksAsync(userId1);

        // Assert
        result.Count.ShouldBe(2);
        result.All(b => b.Title == "Book 1" || b.Title == "Book 2").ShouldBeTrue();
    }

    [Test]
    public async Task GetBookAsync_ReturnsNullForNonMatchingUserId()
    {
        // Arrange
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        List<BookEntity> entities = [
            new() { Id = bookId, Title = "Book 1", Author = "Author 1", ISBN = "12345678901234", PublishDate = DateTime.Now, UserId = Guid.Parse("00000000-0000-0000-0000-000000000003") }
        ];

        IQueryable<BookEntity> queryable = entities.AsQueryable();

        DbSet<BookEntity> booksDbSet = Substitute.For<DbSet<BookEntity>, IQueryable<BookEntity>>();
        ((IQueryable<BookEntity>)booksDbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<BookEntity>)booksDbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<BookEntity>)booksDbSet).ElementType.Returns(queryable.ElementType);

        ShelflyDbContext context = Substitute.For<ShelflyDbContext>(Arg.Any<DbContextOptions<ShelflyDbContext>>());
        context.Books.Returns(booksDbSet);

        BookService service = new(context);

        // Act
        Book? result = await service.GetBookAsync(userId, bookId);

        // Assert
        result.ShouldBeNull();
    }

    [Test]
    public async Task DeleteBookAsync_ChecksUserIdOwnership()
    {
        // Arrange
        Guid userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
        Guid bookId = Guid.Parse("00000000-0000-0000-0000-000000000002");

        List<BookEntity> entities = [
            new() { Id = bookId, Title = "Book 1", Author = "Author 1", ISBN = "12345678901234", PublishDate = DateTime.Now, UserId = Guid.Parse("00000000-0000-0000-0000-000000000003") }
        ];

        IQueryable<BookEntity> queryable = entities.AsQueryable();

        DbSet<BookEntity> booksDbSet = Substitute.For<DbSet<BookEntity>, IQueryable<BookEntity>>();
        ((IQueryable<BookEntity>)booksDbSet).Provider.Returns(queryable.Provider);
        ((IQueryable<BookEntity>)booksDbSet).Expression.Returns(queryable.Expression);
        ((IQueryable<BookEntity>)booksDbSet).ElementType.Returns(queryable.ElementType);

        ShelflyDbContext context = Substitute.For<ShelflyDbContext>(Arg.Any<DbContextOptions<ShelflyDbContext>>());
        context.Books.Returns(booksDbSet);

        BookService service = new(context);

        // Act
        await service.DeleteBookAsync(userId, new Book { Id = bookId });

        // Assert - entity should be removed from context if ownership matches
        context.Books.Received().Remove(Arg.Any<BookEntity>());
    }
}
