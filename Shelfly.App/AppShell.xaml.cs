namespace Shelfly.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
    }

    private async void OnAboutTapped(object? sender, TappedEventArgs e)
    {
        await Current.GoToAsync(Routes.AboutPage);
        FlyoutIsPresented = false;
    }
}