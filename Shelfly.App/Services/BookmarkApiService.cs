using System.Net.Http.Json;
using Shelfly.Common.DTOs;

namespace Shelfly.App.Services;

public class BookmarkApiService(HttpClient httpClient)
{
    public async Task<List<Bookmark>> GetBookmarksAsync(Guid bookId)
    {
        return await httpClient.GetFromJsonAsync<List<Bookmark>>($"/api/bookmarks/{bookId}") ?? [];
    }

    public async Task<bool> CreateBookmarkAsync(Guid bookId, Bookmark bookmark)
    {
        HttpResponseMessage response = await httpClient.PostAsJsonAsync($"/api/bookmarks/{bookId}", bookmark);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateBookmarkAsync(Bookmark bookmark)
    {
        HttpResponseMessage response = await httpClient.PutAsJsonAsync($"/api/bookmarks/{bookmark.Id}", bookmark);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteBookmarkAsync(Guid bookmarkId)
    {
        HttpResponseMessage response = await httpClient.DeleteAsync($"/api/bookmarks/{bookmarkId}");
        return response.IsSuccessStatusCode;
    }
}
