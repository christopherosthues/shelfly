using NLog;
using Shelfly.App.Data;

namespace Shelfly.App;

public partial class App : Application
{
    private readonly LocalDbContext _localDbContext;
    private readonly AppShellViewModel _appShellViewModel;

    public App(LocalDbContext localDbContext, AppShellViewModel  appShellViewModel)
    {
        _localDbContext = localDbContext;
        _appShellViewModel = appShellViewModel;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        LoadingPage loadingPage = new();
        Window window = new Window(loadingPage);

        loadingPage.RetryRequested += async (_, _) => await InitializeAsync(window, loadingPage);

        _ = InitializeAsync(window, loadingPage);
        return window;
    }

    private async Task InitializeAsync(Window window, LoadingPage loadingPage)
    {
        try
        {
            await _localDbContext.EnsureDatabaseCreatedAsync();

            window.Page = new AppShell(_appShellViewModel);
        }
        catch (Exception exception)
        {
            LogManager.GetCurrentClassLogger().Error(exception, "Failed to initialize the local database.");
            loadingPage.ShowError();
        }
    }
}
