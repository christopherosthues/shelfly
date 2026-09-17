using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Settings.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;

namespace Shelfly.App.Features.Settings;

public partial class ServerEntryViewModel(SettingsService settingsService) : ShelflyViewModelBase, IQueryAttributable
{
    [ObservableProperty]
    public partial string Url { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsUrlValid { get; set; }

    [ObservableProperty]
    public partial string Username { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ConfirmPassword { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsTestingConnection { get; set; }

    [ObservableProperty]
    public partial bool IsRegistering { get; set; }

    [ObservableProperty]
    public partial bool IsLoggingIn { get; set; }

    [ObservableProperty]
    public partial string? ConnectionResultMessage { get; set; }

    [ObservableProperty]
    public partial string? RegistrationResultMessage { get; set; }

    [ObservableProperty]
    public partial string? LoginResultMessage { get; set; }

    [ObservableProperty]
    public partial bool ShowRegistrationForm { get; set; }

    [ObservableProperty]
    public partial bool ShowLoginForm { get; set; }

    private Guid _selectedServerEntryId;

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        // Check if we're in login mode (navigating from a saved server selection)
        if (_selectedServerEntryId != Guid.Empty)
        {
            ShowLoginForm = true;
            ShowRegistrationForm = false;

            // Load the server URL for display
            SavedServerEntry? entry = await settingsService.GetSavedServerEntryByIdAsync(_selectedServerEntryId, cancellationToken);
            if (entry?.Server is not null)
            {
                Url = entry.Server.Url;
            }
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("serverEntryId", out object? serverEntryIdObj) && serverEntryIdObj is Guid parsedEntryId)
        {
            _selectedServerEntryId = parsedEntryId;
        }

        if (query.TryGetValue(nameof(NavigationMode), out object? mode) && mode?.ToString() == "Login")
        {
            ShowLoginForm = true;
            ShowRegistrationForm = false;
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        IsTestingConnection = true;
        ConnectionResultMessage = null;

        try
        {
            ApiResult<bool> result = await settingsService.TestConnectionAsync(Url);

            if (result.IsSuccess)
            {
                IsUrlValid = true;
                ConnectionResultMessage = AppResources.ServerEntryPageConnectionSuccessText;
                ShowRegistrationForm = true;

                await Shell.Current.DisplayAlertAsync(
                    AppResources.CommonOkButton,
                    AppResources.ServerEntryPageConnectionSuccessText,
                    AppResources.CommonOkButton);
            }
            else
            {
                ConnectionResultMessage = result.ErrorMessage ?? AppResources.ServerEntryPageConnectionFailureText;

                await Shell.Current.DisplayAlertAsync(
                    AppResources.CommonErrorTitle,
                    ConnectionResultMessage,
                    AppResources.CommonOkButton);
            }
        }
        catch (Exception ex)
        {
            ConnectionResultMessage = ex.Message;

            await Shell.Current.DisplayAlertAsync(
                AppResources.CommonErrorTitle,
                ConnectionResultMessage,
                AppResources.CommonOkButton);
        }
        finally
        {
            IsTestingConnection = false;
        }
    }

    [RelayCommand]
    private async Task RegisterAsync()
    {
        if (Password != ConfirmPassword)
        {
            RegistrationResultMessage = AppResources.ServerEntryPagePasswordMismatchError;

            await Shell.Current.DisplayAlertAsync(
                AppResources.CommonErrorTitle,
                RegistrationResultMessage,
                AppResources.CommonOkButton);

            return;
        }

        IsRegistering = true;
        RegistrationResultMessage = null;

        try
        {
            ApiResult<SavedServerEntry> result = await settingsService.RegisterAndSaveServerAsync(Url, Username, Email, Password);

            if (result.IsSuccess)
            {
                // Store credentials
                settingsService.SaveCredentials(result.Value!.Id, Username, Email, string.Empty);

                RegistrationResultMessage = AppResources.SettingsPageRegistrationSuccessMessage;

                await Shell.Current.DisplayAlertAsync(
                    AppResources.CommonSuccessTitle,
                    RegistrationResultMessage,
                    AppResources.CommonOkButton);

                // Navigate back to Settings page
                await Shell.Current.GoToAsync($"..?{nameof(NavigationMode)}=Back");
            }
            else
            {
                RegistrationResultMessage = result.ErrorMessage ?? AppResources.SettingsPageRegistrationFailureMessage;

                await Shell.Current.DisplayAlertAsync(
                    AppResources.CommonErrorTitle,
                    RegistrationResultMessage,
                    AppResources.CommonOkButton);
            }
        }
        catch (Exception ex)
        {
            RegistrationResultMessage = ex.Message;

            await Shell.Current.DisplayAlertAsync(
                AppResources.CommonErrorTitle,
                RegistrationResultMessage,
                AppResources.CommonOkButton);
        }
        finally
        {
            IsRegistering = false;
        }
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (_selectedServerEntryId == Guid.Empty)
        {
            LoginResultMessage = AppResources.ServerEntryPageGenericAuthError;

            await Shell.Current.DisplayAlertAsync(
                AppResources.CommonErrorTitle,
                LoginResultMessage,
                AppResources.CommonOkButton);

            return;
        }

        IsLoggingIn = true;
        LoginResultMessage = null;

        try
        {
            ApiResult<bool> result = await settingsService.SignInExistingServerAsync(_selectedServerEntryId, Username, Password);

            if (result.IsSuccess)
            {
                // Store credentials
                settingsService.SaveCredentials(_selectedServerEntryId, Username, string.Empty, string.Empty);

                LoginResultMessage = AppResources.SettingsPageRegistrationSuccessMessage;

                await Shell.Current.DisplayAlertAsync(
                    AppResources.CommonSuccessTitle,
                    LoginResultMessage,
                    AppResources.CommonOkButton);

                // Navigate back to Settings page
                await Shell.Current.GoToAsync($"..?{nameof(NavigationMode)}=Back");
            }
            else
            {
                LoginResultMessage = result.ErrorMessage ?? AppResources.ServerEntryPageGenericAuthError;

                await Shell.Current.DisplayAlertAsync(
                    AppResources.CommonErrorTitle,
                    LoginResultMessage,
                    AppResources.CommonOkButton);
            }
        }
        catch (Exception ex)
        {
            LoginResultMessage = ex.Message;

            await Shell.Current.DisplayAlertAsync(
                AppResources.CommonErrorTitle,
                LoginResultMessage,
                AppResources.CommonOkButton);
        }
        finally
        {
            IsLoggingIn = false;
        }
    }

    public const string NavigationMode = "NavigationMode";
}