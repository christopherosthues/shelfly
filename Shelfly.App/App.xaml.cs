using NLog;
using Shelfly.App.Data;

namespace Shelfly.App;

public partial class App : Application
{
    private readonly LocalDbContext _localDbContext;

    public App(LocalDbContext localDbContext)
    {
        _localDbContext = localDbContext;
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

            window.Page = new AppShell();
        }
        catch (Exception exception)
        {
            LogManager.GetCurrentClassLogger().Error(exception, "Failed to initialize the local database.");
            loadingPage.ShowError();
        }
    }
}
