using System.Reflection;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;
using Shelfly.Common;

namespace Shelfly.App.Features.About;

public partial class AboutViewModel(LicenseDataService licenseService) : ShelflyViewModelBase
{
    [ObservableProperty] public partial string AppVersion { get; set; } = string.Empty;

    [ObservableProperty] public partial string BuildInfo { get; set; } = string.Empty;

    [ObservableProperty] public partial List<DependencyPackage> Dependencies { get; set; } = [];

    [ObservableProperty] public partial bool IsDependenciesLoading { get; set; }

    [ObservableProperty] public partial string? DependenciesErrorMessage { get; set; }

    [ObservableProperty] public partial string SelectedLicensePackageId { get; set; } = string.Empty;

    [ObservableProperty] public partial string? LicenseText { get; set; }

    [ObservableProperty] public partial bool IsLicenseTextLoading { get; set; }

    [ObservableProperty] public partial string? LicenseTextErrorMessage { get; set; }

    protected override Task LoadAsync(CancellationToken cancellationToken)
    {
        Assembly entryAssembly = Assembly.GetExecutingAssembly();

        Version? version = entryAssembly.GetName().Version;
        AppVersion = version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "Unknown";

        string? informationalVersion = entryAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;
        BuildInfo = informationalVersion ?? "Unknown build";

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ShowLicenseAsync(DependencyPackage? package, CancellationToken cancellationToken)
    {
        if (package is null)
        {
            return;
        }

        if (package.IsFileBased)
        {
            await ShowLicenseTextPopupAsync(package, cancellationToken);

            return;
        }

        if (package.LicenseUrl is not null)
        {
            await Launcher.OpenAsync(package.LicenseUrl);

            return;
        }

        if (Shell.Current is not null)
        {
            await Shell.Current.DisplayAlertAsync(
                AppResources.AboutPageLibrariesTitle,
                AppResources.AboutPageLicenseUnavailableMessage,
                AppResources.CommonOkButton);
        }
    }

    private async Task ShowLicenseTextPopupAsync(DependencyPackage package, CancellationToken cancellationToken)
    {
        if (Shell.Current is null)
        {
            return;
        }

        SelectedLicensePackageId = package.PackageId;
        LicenseText = null;
        LicenseTextErrorMessage = null;
        IsLicenseTextLoading = true;

        LicenseTextPopup popup = new(this);
        Task<IPopupResult> showPopupTask = Shell.Current.ShowPopupAsync(popup, options: null, token: cancellationToken);

        Result<string> result = await licenseService.LoadLicenseFileContentAsync(package, cancellationToken);

        if (result.IsSuccess)
        {
            LicenseText = result.Value;
        }
        else
        {
            LicenseTextErrorMessage = AppResources.AboutPageLicenseFileErrorMessage;
        }

        IsLicenseTextLoading = false;

        await showPopupTask;
    }

    [RelayCommand]
    private async Task ShowLibrariesDialogAsync(CancellationToken cancellationToken)
    {
        if (Shell.Current is null)
        {
            return;
        }

        IsDependenciesLoading = true;
        DependenciesErrorMessage = null;
        Dependencies = [];

        DependenciesPopup popup = new(this);
        Task<IPopupResult> showPopupTask = Shell.Current.ShowPopupAsync(popup, options: null, token: cancellationToken);

        Result<List<DependencyPackage>> result = await licenseService.LoadDependenciesAsync(cancellationToken);

        if (result.IsSuccess)
        {
            Dependencies = result.Value;
        }
        else
        {
            DependenciesErrorMessage = result.Error;
        }

        IsDependenciesLoading = false;

        await showPopupTask;
    }
}