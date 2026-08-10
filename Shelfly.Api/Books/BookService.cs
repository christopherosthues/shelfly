using Shelfly.Common;

namespace Shelfly.Api.Books;

public class BookService
{
    public async Task<List<Book>> GetBooks(Guid userId)
    {
        return [];
    }

    public async Task<Book?> GetBook(Guid userId, Guid bookId)
    {
        Bogus.Faker<Book> bookFaker = new Bogus.Faker<Book>();
        Book book = bookFaker.Generate();
        return book;
    }

    public async Task AddBook(Guid userId, Book book)
    {
        await Task.CompletedTask;
    }

    public async Task UpdateBook(Guid userId, Book book)
    {
        await Task.CompletedTask;
    }

    public async Task DeleteBook(Guid userId, Book book)
    {
        await Task.CompletedTask;
    }
}