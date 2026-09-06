using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;

namespace Shelfly.App.Features.About;

public partial class AboutViewModel : ShelflyViewModelBase
{
    [ObservableProperty] public partial string AppVersion { get; set; } = string.Empty;

    [ObservableProperty] public partial string BuildInfo { get; set; } = string.Empty;

    protected override Task LoadAsync(CancellationToken cancellationToken)
    {
        Assembly entryAssembly = Assembly.GetExecutingAssembly();

        Version? version = entryAssembly.GetName().Version;
        AppVersion = version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "Unknown";

        string? informationalVersion = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        BuildInfo = informationalVersion ?? "Unknown build";

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ShowLibrariesDialogAsync()
    {
        string librariesText = @"CommunityToolkit.Mvvm (MIT License)
  - Version: 8.4.2
  - URL: https://github.com/CommunityToolkit/dotnet

CommunityToolkit.Maui (MIT License)
  - Version: 15.0.1
  - URL: https://github.com/CommunityToolkit/Maui

Microsoft.Maui.Controls (MIT License)
  - Version: 10.0.100
  - URL: https://github.com/dotnet/maui

Microsoft.EntityFrameworkCore.Sqlite (Apache-2.0 License)
  - Version: 10.0.11
  - URL: https://github.com/dotnet/efcore

SQLitePCLRaw.bundle_e_sqlite3 (BSD-3-Clause / Apache-2.0)
  - Version: 2.1.12
  - URL: https://github.com/ericsink/SQLCipher

NLog (BSD-2-Clause License)
  - Version: 6.2.0
  - URL: https://github.com/NLog/NLog";

        await Shell.Current.DisplayAlertAsync(AppResources.AboutPageLibrariesTitle, librariesText, AppResources.CommonOkButton);
    }
}
