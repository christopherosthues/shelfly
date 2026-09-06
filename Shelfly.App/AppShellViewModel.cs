using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Shelfly.App;

public partial class AppShellViewModel : ObservableObject
{
    [RelayCommand]
    private async Task NavigateToAboutAsync()
    {
        await Shell.Current.GoToAsync(Routes.AboutPage);
        Shell.Current.FlyoutIsPresented = false;
    }
}
