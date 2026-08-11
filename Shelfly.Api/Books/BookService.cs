using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Data.Entities;
using Shelfly.Common;

namespace Shelfly.Api.Books;

public class BookService(AppDbContext context)
{
    public async Task<List<Book>> GetBooks(Guid userId)
    {
        return await context.Books
            .Select(b => new Book
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                ISBN = b.ISBN,
                PublishDate = b.PublishDate
            })
            .ToListAsync();
    }

    public async Task<Book?> GetBook(Guid userId, Guid bookId)
    {
        BookEntity? entity = await context.Books.FirstOrDefaultAsync(b => b.Id == bookId);
        return entity is null ? null : new Book
        {
            Id = entity.Id,
            Title = entity.Title,
            Author = entity.Author,
            ISBN = entity.ISBN,
            PublishDate = entity.PublishDate
        };
    }

    public async Task AddBook(Guid userId, Book book)
    {
        BookEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PublishDate = book.PublishDate
        };

        await context.Books.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBook(Guid userId, Book book)
    {
        BookEntity? entity = await context.Books.FirstOrDefaultAsync(b => b.Id == book.Id);
        if (entity != null)
        {
            entity.Title = book.Title;
            entity.Author = book.Author;
            entity.ISBN = book.ISBN;
            entity.PublishDate = book.PublishDate;

            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteBook(Guid userId, Book book)
    {
        BookEntity? entity = await context.Books.FirstOrDefaultAsync(b => b.Id == book.Id);
        if (entity != null)
        {
            context.Books.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
