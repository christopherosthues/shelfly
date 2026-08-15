using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http.Json;
using Shelfly.App.Routing;

namespace Shelfly.App.Authentication;

public partial class RegistrationViewModel(IHttpClientFactory httpClientFactory) : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _userName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _password = string.Empty;

    public bool CanRegister => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);

    [RelayCommand(CanExecute = nameof(CanRegister))]
    private async Task RegisterAsync()
    {
        HttpClient httpClient = new();
        httpClient.BaseAddress = new("http://localhost:5000/");

        HttpResponseMessage response = await httpClient.PostAsJsonAsync("/auth/register", new { Email = UserName, Password = Password });

        if (response.StatusCode == System.Net.HttpStatusCode.Created)
        {
            await Shell.Current.GoToAsync(Routes.LoginPage);
        }
    }

    [RelayCommand]
    private async Task GoToLoginAsync()
    {
        await Shell.Current.GoToAsync(Routes.LoginPage);
    }
}