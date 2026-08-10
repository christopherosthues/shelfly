using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shelfly.App.Authentication;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _userName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string _password = string.Empty;

    public bool CanLogin => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);

    [RelayCommand(CanExecute = nameof(CanLogin))]
    public async Task LoginAsync()
    {
        HttpClient httpClient = new HttpClient();

        // await httpClient.PostAsJsonAsync();
    }

    [RelayCommand]
    public async Task GoToRegistration()
    {

    }
}