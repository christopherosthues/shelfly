using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shelfly.App;

public partial class AppShellViewModel : ObservableObject
{
    [RelayCommand]
    private async Task NavigateToAboutAsync()
    {
        Shell.Current.FlyoutIsPresented = false;
        await Shell.Current.GoToAsync(Routes.AboutPage);
    }
}
