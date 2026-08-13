using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shelfly.App.Authentication;

public partial class RegistrationViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _userName = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(RegisterCommand))]
    private string _password = string.Empty;

    public bool CanRegister => !string.IsNullOrEmpty(UserName) && !string.IsNullOrEmpty(Password);

    [RelayCommand(CanExecute = nameof(CanRegister))]
    public async Task RegisterAsync()
    {
        HttpClient httpClient = new HttpClient();

        // await httpClient.PostAsJsonAsync();
    }

    [RelayCommand]
    public async Task GoToLoginAsync()
    {

    }
}