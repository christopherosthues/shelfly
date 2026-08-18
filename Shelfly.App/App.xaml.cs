using Microsoft.EntityFrameworkCore;
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
        _localDbContext.EnsureDatabaseCreatedAsync().ContinueWith(_ =>
        {
            Current?.Dispatcher.Dispatch(async () =>
            {
                await _localDbContext.Database.MigrateAsync();
            });
        }, TaskContinuationOptions.OnlyOnFaulted);

        return new(new AppShell());
    }
}