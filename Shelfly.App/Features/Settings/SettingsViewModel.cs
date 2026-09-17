using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Settings.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;

namespace Shelfly.App.Features.Settings;

public partial class SettingsViewModel(SettingsService settingsService, SyncService syncService) : ShelflyViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<SavedServerEntry> SavedServers { get; set; } = [];

    [ObservableProperty]
    public partial SavedServerEntry? ActiveServerEntry { get; set; }

    [ObservableProperty]
    public partial bool SyncEnabled { get; set; }

    [ObservableProperty]
    public partial DateTime? LastSuccessfulSyncAt { get; set; }

    [ObservableProperty]
    public partial string? LastSyncResult { get; set; }

    [ObservableProperty]
    public partial bool IsSyncing { get; set; }

    [ObservableProperty]
    public partial bool HasActiveServer { get; set; }

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        // Load saved servers
        List<SavedServerEntry> servers = await settingsService.GetSavedServersAsync(cancellationToken);
        SavedServers = new ObservableCollection<SavedServerEntry>(servers);

        // Load sync state
        SyncState syncState = await settingsService.GetSyncStateAsync(cancellationToken);
        SyncEnabled = syncState.SyncEnabled;
        LastSuccessfulSyncAt = syncState.LastSuccessfulSyncAt;
        LastSyncResult = syncState.LastSyncResult;

        // Find active server entry
        if (syncState.ActiveServerEntryId.HasValue)
        {
            ActiveServerEntry = SavedServers.FirstOrDefault(e => e.Id == syncState.ActiveServerEntryId.Value);
        }

        HasActiveServer = ActiveServerEntry is not null;
    }

    [RelayCommand]
    private static async Task AddServerAsync()
    {
        await Shell.Current.GoToAsync(Routes.ServerEntryPage);
    }

    [RelayCommand]
    private async Task SignInAsync(SavedServerEntry serverEntry)
    {
        // Navigate to ServerEntryPage with the selected server entry ID
        await Shell.Current.GoToAsync($"{Routes.ServerEntryPage}?{nameof(ServerEntryViewModel.NavigationMode)}=Login&serverEntryId={serverEntry.Id}");
    }

    [RelayCommand]
    private async Task ToggleSyncAsync()
    {
        SyncState syncState = await settingsService.GetSyncStateAsync();
        bool newSyncEnabled = !syncState.SyncEnabled;

        // Update sync state via service (handles SaveChanges internally)
        await settingsService.UpdateSyncStateAsync(newSyncEnabled, syncState.ActiveServerEntryId);
        SyncEnabled = newSyncEnabled;

        // If turning on and there's an active server, trigger immediate sync
        if (SyncEnabled && syncState.ActiveServerEntryId.HasValue)
        {
            IsSyncing = true;

            try
            {
                ApiResult<string> result = await syncService.SyncAsync();

                LastSuccessfulSyncAt = result.IsSuccess ? DateTime.UtcNow : null;
                LastSyncResult = result.IsSuccess
                    ? AppResources.SettingsPageSyncSuccessMessage
                    : result.ErrorMessage ?? AppResources.SettingsPageSyncFailureMessage;
            }
            finally
            {
                IsSyncing = false;
            }
        }
    }

    [RelayCommand]
    private async Task SelectServerAsync(SavedServerEntry serverEntry)
    {
        // Set as active using service (handles SaveChanges internally)
        await settingsService.SetActiveServerEntryAsync(serverEntry.Id, syncEnabled: true);
        ActiveServerEntry = serverEntry;

        // Refresh UI
        await LoadAsync(CancellationToken.None);
    }

    [RelayCommand]
    private async Task SignOutAsync()
    {
        // Clear active entry using service (handles SaveChanges internally)
        await settingsService.ClearActiveServerEntryAsync();

        ActiveServerEntry = null;
        SyncEnabled = false;
        HasActiveServer = false;

        await Shell.Current.DisplayAlertAsync(
            AppResources.CommonSuccessTitle,
            AppResources.SettingsPageSignOutSuccessMessage,
            AppResources.CommonOkButton);
    }

    [RelayCommand]
    private async Task SyncNowAsync()
    {
        if (ActiveServerEntry is null)
        {
            await Shell.Current.DisplayAlertAsync(
                AppResources.CommonInfoTitle,
                AppResources.SettingsPageNoActiveServerMessage,
                AppResources.CommonOkButton);

            return;
        }

        IsSyncing = true;

        try
        {
            ApiResult<string> result = await syncService.SyncAsync();

            LastSuccessfulSyncAt = result.IsSuccess ? DateTime.UtcNow : null;
            LastSyncResult = result.IsSuccess
                ? AppResources.SettingsPageSyncSuccessMessage
                : result.ErrorMessage ?? AppResources.SettingsPageSyncFailureMessage;

            if (result.IsSuccess)
            {
                await Shell.Current.DisplayAlertAsync(
                    AppResources.CommonSuccessTitle,
                    AppResources.SettingsPageSyncSuccessMessage,
                    AppResources.CommonOkButton);
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(
                    AppResources.CommonErrorTitle,
                    LastSyncResult,
                    AppResources.CommonOkButton);
            }
        }
        finally
        {
            IsSyncing = false;
        }
    }
}