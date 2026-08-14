using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Shelfly.Api.Data;
using Shelfly.Api.Tests.Helpers;

namespace Shelfly.Api.Tests.Integration;

public class IntegrationTestBase
{
    [ClassDataSource<ShelflyWebApplicationFactory>(Shared = SharedType.PerClass)]
    public required ShelflyWebApplicationFactory ApiFactory { get; init; }

    protected HttpClient CreateHttpClient()
    {
        HttpClient httpClient = ApiFactory.CreateClient(new() { AllowAutoRedirect = false });
        return httpClient;
    }

    // Keep this commented out code, We will add GraphQL support later
    // protected GraphQLHttpClient CreateUnauthenticatedGraphQLClient()
    // {
    //     HttpClient httpClient = ApiFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    //     GraphQLHttpClient graphQLClient =
    //         new(new GraphQLHttpClientOptions { EndPoint = new Uri("http://localhost:8080/graphql"), },
    //             new SystemTextJsonSerializer(new JsonSerializerOptions
    //             {
    //                 PropertyNameCaseInsensitive = true,
    //                 Converters = { new JsonStringEnumConverter() }
    //             }),
    //             httpClient);
    //     return graphQLClient;
    // }
    //
    // protected async Task<GraphQLHttpClient> CreateAuthenticatedGraphQLClientAsync(
    //     string username = KeycloakConfig.TestUserName, string password = KeycloakConfig.TestPassword)
    // {
    //     HttpClient httpClient = ApiFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    //     await SetAuthorizationHeaderAsync(httpClient);
    //     GraphQLHttpClient graphQLClient =
    //         new(new GraphQLHttpClientOptions { EndPoint = new Uri("http://localhost:8080/graphql"), },
    //             new SystemTextJsonSerializer(new JsonSerializerOptions
    //             {
    //                 PropertyNameCaseInsensitive = true,
    //                 Converters = { new JsonStringEnumConverter() }
    //             }),
    //             httpClient);
    //     return graphQLClient;
    // }

    protected async Task SetAuthorizationHeaderAsync(HttpClient httpClient,
        string username = KeycloakConfig.TestUserName, string password = KeycloakConfig.TestPassword)
    {
        string url = $"{ApiFactory.KeycloakBaseAddress()}/realms/{KeycloakConfig.Realm}/protocol/openid-connect/token";

        Dictionary<string, string> data = new()
        {
            { "grant_type", "password" }, { "client_id", KeycloakConfig.ClientId }, { "username", username }, { "password", password },
        };

        HttpClient tokenClient = new();

        HttpResponseMessage response = await tokenClient.PostAsync(url, new FormUrlEncodedContent(data));
        JsonObject? content = await response.Content.ReadFromJsonAsync<JsonObject>();
        string? token = content?["access_token"]?.ToString();
        httpClient.DefaultRequestHeaders.Authorization = new("Bearer", token);
    }

    protected static void UnsetAuthorizationHeader(HttpClient httpClient)
    {
        httpClient.DefaultRequestHeaders.Authorization = null;
    }

    protected async Task SeedDatabaseAsync(Action<ShelflyDbContext> seed)
    {
        using IServiceScope scope = ApiFactory.Services.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        ShelflyDbContext dbContext = serviceProvider.GetRequiredService<ShelflyDbContext>();
        seed(dbContext);
        await dbContext.SaveChangesAsync();
    }

    protected async Task CheckDbContentAsync(Func<ShelflyDbContext, Task> checkAsync)
    {
        using IServiceScope scope = ApiFactory.Services.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        ShelflyDbContext dbContext = serviceProvider.GetRequiredService<ShelflyDbContext>();
        await checkAsync(dbContext);
    }

    [After(Test)]
    public async Task TearDownBase()
    {
        using IServiceScope scope = ApiFactory.Services.CreateScope();
        IServiceProvider serviceProvider = scope.ServiceProvider;
        ShelflyDbContext dbContext = serviceProvider.GetRequiredService<ShelflyDbContext>();
        await ResetDatabase(dbContext);

        // GraphQLClient.Dispose();
        // await ApiFactory.DisposeAsync();
    }

    /// <summary>
    /// This method reset the database after each test. This is the place where you can clear the database. The default
    /// implementation deletes all data from the database.
    /// </summary>
    /// <param name="dbContext">The database context used to reset the database</param>
    private static async Task ResetDatabase(ShelflyDbContext dbContext)
    {
        dbContext.Bookmarks.RemoveRange(dbContext.Bookmarks);
        dbContext.Books.RemoveRange(dbContext.Books);

        await dbContext.SaveChangesAsync();
    }
}