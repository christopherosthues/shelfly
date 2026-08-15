using System.Net.Http.Json;
using Shelfly.Common.DTOs;

namespace Shelfly.App.Services;

public class BookApiService(HttpClient httpClient)
{
    public async Task<List<Book>> GetBooksAsync()
    {
        return await httpClient.GetFromJsonAsync<List<Book>>("/api/books") ?? [];
    }

    public async Task<Book?> GetBookAsync(Guid bookId)
    {
        return await httpClient.GetFromJsonAsync<Book>($"/api/books/{bookId}");
    }

    public async Task<bool> CreateBookAsync(Book book)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/api/books", book);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateBookAsync(Book book)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/api/books/{book.Id}", book);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBookAsync(Guid bookId)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"/api/books/{bookId}");
        return response.IsSuccessStatusCode;
    }
}
