using System.Net.Http.Json;
using Shelfly.App.Data.Entities;
using Shelfly.Common;

namespace Shelfly.App.Features.Settings.Services;

public class ApiClient(IHttpClientFactory httpClientFactory)
{
    public async Task<Result<bool>> TestConnectionAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(url);
            HttpResponseMessage response = await httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode
                ? Result<bool>.Success(true)
                : Result<bool>.Failure("Server returned unexpected status");
        }
        catch (TaskCanceledException)
        {
            return Result<bool>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return Result<bool>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<Result<string>> RegisterAsync(string url, string username, string email, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress  = new Uri(url);
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/auth/register",
                new { Username = username, Email = email, Password = password }, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string token = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result<string>.Success(token.Trim('"'));
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<string>.Failure($"Registration failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return Result<string>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<Result<string>> LoginAsync(string url, string username, string password, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(url);
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/auth/login",
                new { Username = username, Password = password }, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string token = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result<string>.Success(token.Trim('"'));
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<string>.Failure($"Sign in failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return Result<string>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/auth/logout", new { }, cancellationToken);
            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (TaskCanceledException)
        {
            return Result<bool>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return Result<bool>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<Result<List<BookEntity>>> FetchServerBooksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.GetAsync("/v1/sync/books", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                List<BookEntity>? books = await response.Content.ReadFromJsonAsync<List<BookEntity>>(cancellationToken: cancellationToken);
                return Result<List<BookEntity>>.Success(books ?? []);
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<List<BookEntity>>.Failure($"Fetch failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return Result<List<BookEntity>>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return Result<List<BookEntity>>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UploadBookAsync(BookEntity book, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/sync/books", book, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string serverId = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result<string>.Success(serverId.Trim('"'));
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<string>.Failure($"Upload failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return Result<string>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<Result<bool>> UpdateBookAsync(BookEntity book, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PutAsJsonAsync("/v1/sync/books", book, cancellationToken);
            return Result<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (TaskCanceledException)
        {
            return Result<bool>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return Result<bool>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<Result<List<BookmarkEntity>>> FetchServerBookmarksAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        try
        {using HttpClient httpClient = httpClientFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync($"/v1/sync/books/{bookId}/bookmarks", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                List<BookmarkEntity>? bookmarks = await response.Content.ReadFromJsonAsync<List<BookmarkEntity>>(cancellationToken: cancellationToken);
                return Result<List<BookmarkEntity>>.Success(bookmarks ?? []);
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<List<BookmarkEntity>>.Failure($"Fetch failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return Result<List<BookmarkEntity>>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return Result<List<BookmarkEntity>>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<Result<string>> UploadBookmarkAsync(BookmarkEntity bookmark, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/sync/bookmarks", bookmark, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string serverId = await response.Content.ReadAsStringAsync(cancellationToken);
                return Result<string>.Success(serverId.Trim('"'));
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<string>.Failure($"Upload failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return Result<string>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return Result<string>.Failure($"Network error: {ex.Message}");
        }
    }
}
