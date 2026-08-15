using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;
using Shelfly.App.Routing;
using Shelfly.App.Services;

namespace Shelfly.App.Authentication;

public partial class LoginViewModel(IHttpClientFactory httpClientFactory) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _userName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    public bool CanLogin => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);

    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        using HttpClient httpClient = httpClientFactory.CreateClient();
        httpClient.BaseAddress = new("http://localhost:5000/");

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/auth/login", new { Email = UserName, Password = Password });

        if (response.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Dictionary<string, string>? body = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            string? accessToken = body?.GetValueOrDefault("accessToken");
            string? refreshToken = body?.GetValueOrDefault("refreshToken");

            if (accessToken is not null)
            {
                await SecureTokenStore.StoreTokensAsync(accessToken, refreshToken);
                await Shell.Current.GoToAsync("../" + Routes.BooksPage);
            }
        }
    }

    [RelayCommand]
    private async Task GoToRegistrationAsync()
    {
        await Shell.Current.GoToAsync(Routes.RegistrationPage);
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        string? accessToken = await SecureTokenStore.GetAccessTokenAsync();
        if (accessToken is not null)
        {
            using HttpClient httpClient = httpClientFactory.CreateClient();
            httpClient.BaseAddress = new("http://localhost:5000/");

            using HttpRequestMessage request = new(HttpMethod.Post, "http://localhost:5000/auth/logout");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");

            await httpClient.SendAsync(request);
        }

        SecureTokenStore.RemoveTokens();
    }
}