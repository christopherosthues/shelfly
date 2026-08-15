using Microsoft.EntityFrameworkCore;
using Shelfly.Api.Data;
using Shelfly.Api.Data.Entities;
using Shelfly.Common.DTOs;

namespace Shelfly.Api.Books;

public class BookService(ShelflyDbContext context)
{
    public async Task<List<Book>> GetBooksAsync(Guid userId)
    {
        return await context.Books
            .Where(b => b.UserId == userId)
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

    public async Task<Book?> GetBookAsync(Guid userId, Guid bookId)
    {
        BookEntity? entity = await context.Books
            .Include(b => b.Bookmarks)
            .FirstOrDefaultAsync(b => b.Id == bookId && b.UserId == userId);

        return entity is null ? null : new Book
        {
            Id = entity.Id,
            Title = entity.Title,
            Author = entity.Author,
            ISBN = entity.ISBN,
            PublishDate = entity.PublishDate,
            Bookmarks = entity.Bookmarks
                .Where(bm => bm.UserId == userId)
                .Select(bm => new Bookmark
                {
                    Id = bm.Id,
                    StartPage = bm.StartPage,
                    EndPage = bm.EndPage,
                    Note = bm.Note
                })
                .ToList()
        };
    }

    public async Task AddBookAsync(Guid userId, Book book)
    {
        BookEntity entity = new()
        {
            Id = Guid.NewGuid(),
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            PublishDate = book.PublishDate,
            UserId = userId
        };

        await context.Books.AddAsync(entity);
        await context.SaveChangesAsync();
    }

    public async Task UpdateBookAsync(Guid userId, Book book)
    {
        BookEntity? entity = await context.Books.FirstOrDefaultAsync(b => b.Id == book.Id && b.UserId == userId);
        if (entity != null)
        {
            entity.Title = book.Title;
            entity.Author = book.Author;
            entity.ISBN = book.ISBN;
            entity.PublishDate = book.PublishDate;

            await context.SaveChangesAsync();
        }
    }

    public async Task DeleteBookAsync(Guid userId, Book book)
    {
        BookEntity? entity = await context.Books.FirstOrDefaultAsync(b => b.Id == book.Id && b.UserId == userId);
        if (entity != null)
        {
            context.Books.Remove(entity);
            await context.SaveChangesAsync();
        }
    }
}
