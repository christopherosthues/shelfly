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
        Window window = new Window(new LoadingPage());

        _ = InitializeAsync(window);
        return window;
    }

    private async Task InitializeAsync(Window window)
    {
        await _localDbContext.EnsureDatabaseCreatedAsync();

        window.Page = new AppShell();
    }
}