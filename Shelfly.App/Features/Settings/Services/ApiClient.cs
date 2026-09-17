using System.Net.Http.Json;
using Shelfly.App.Data.Entities;

namespace Shelfly.App.Features.Settings.Services;

public class ApiClient(IHttpClientFactory httpClientFactory)
{
    // TODO: ApiClient got really fucked up by the AI
    // TODO: retrieve correct url for the clients and set them
    // TODO: Dtos and maybe client should be generated from the OpenAPI specification using Kiota
    // TODO: Instead of an ApiResult<T> use the existing Result<T> class -> duplicated code

    public async Task<ApiResult<bool>> TestConnectionAsync(string url, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri(url);
            HttpResponseMessage response = await httpClient.GetAsync("/health", cancellationToken);
            return response.IsSuccessStatusCode
                ? ApiResult<bool>.Success(true)
                : ApiResult<bool>.Failure("Server returned unexpected status");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<bool>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return ApiResult<bool>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<string>> RegisterAsync(string url, string username, string email, string password, CancellationToken cancellationToken = default)
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
                return ApiResult<string>.Success(token.Trim('"'));
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return ApiResult<string>.Failure($"Registration failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<string>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return ApiResult<string>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<string>> LoginAsync(string url, string username, string password, CancellationToken cancellationToken = default)
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
                return ApiResult<string>.Success(token.Trim('"'));
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return ApiResult<string>.Failure($"Sign in failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<string>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return ApiResult<string>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<bool>> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/auth/logout", new { }, cancellationToken);
            return ApiResult<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (TaskCanceledException)
        {
            return ApiResult<bool>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return ApiResult<bool>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<List<BookEntity>>> FetchServerBooksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.GetAsync("/v1/sync/books", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                List<BookEntity>? books = await response.Content.ReadFromJsonAsync<List<BookEntity>>(cancellationToken: cancellationToken);
                return ApiResult<List<BookEntity>>.Success(books ?? []);
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return ApiResult<List<BookEntity>>.Failure($"Fetch failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<List<BookEntity>>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return ApiResult<List<BookEntity>>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<string>> UploadBookAsync(BookEntity book, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/sync/books", book, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string serverId = await response.Content.ReadAsStringAsync(cancellationToken);
                return ApiResult<string>.Success(serverId.Trim('"'));
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return ApiResult<string>.Failure($"Upload failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<string>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return ApiResult<string>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<bool>> UpdateBookAsync(BookEntity book, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PutAsJsonAsync("/v1/sync/books", book, cancellationToken);
            return ApiResult<bool>.Success(response.IsSuccessStatusCode);
        }
        catch (TaskCanceledException)
        {
            return ApiResult<bool>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return ApiResult<bool>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<List<BookmarkEntity>>> FetchServerBookmarksAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        try
        {using HttpClient httpClient = httpClientFactory.CreateClient();

            HttpResponseMessage response = await httpClient.GetAsync($"/v1/sync/books/{bookId}/bookmarks", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                List<BookmarkEntity>? bookmarks = await response.Content.ReadFromJsonAsync<List<BookmarkEntity>>(cancellationToken: cancellationToken);
                return ApiResult<List<BookmarkEntity>>.Success(bookmarks ?? []);
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return ApiResult<List<BookmarkEntity>>.Failure($"Fetch failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<List<BookmarkEntity>>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return ApiResult<List<BookmarkEntity>>.Failure($"Network error: {ex.Message}");
        }
    }

    public async Task<ApiResult<string>> UploadBookmarkAsync(BookmarkEntity bookmark, CancellationToken cancellationToken = default)
    {
        try
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            HttpResponseMessage response = await httpClient.PostAsJsonAsync("/v1/sync/bookmarks", bookmark, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                string serverId = await response.Content.ReadAsStringAsync(cancellationToken);
                return ApiResult<string>.Success(serverId.Trim('"'));
            }

            string errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
            return ApiResult<string>.Failure($"Upload failed: {errorBody}");
        }
        catch (TaskCanceledException)
        {
            return ApiResult<string>.Failure("Connection timed out");
        }
        catch (IOException ex)
        {
            return ApiResult<string>.Failure($"Network error: {ex.Message}");
        }
    }
}
