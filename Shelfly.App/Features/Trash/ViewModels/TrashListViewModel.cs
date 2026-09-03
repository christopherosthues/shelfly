using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Library.Services;
using Shelfly.App.Features.Trash.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;

namespace Shelfly.App.Features.Trash.ViewModels;

public partial class TrashListViewModel(TrashService trashService) : SortableListViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyStateMessage))]
    public partial ObservableCollection<BookEntity> TrashBooks { get; set; } = [];

    [ObservableProperty]
    public partial bool IsSelectionMode { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<Guid> SelectedItemIds { get; set; } = [];

    public string EmptyStateMessage => TrashBooks.Count == 0
        ? (string.IsNullOrWhiteSpace(SearchQuery)
            ? AppResources.TrashListPageEmptyStateMessage
            : AppResources.TrashListPageSearchEmptyMessage)
        : string.Empty;

    public bool IsRestoreAllVisible => !IsSelectionMode && TrashBooks.Any();
    public bool IsDeleteAllVisible => !IsSelectionMode && TrashBooks.Any();
    public bool IsRestoreSelectedVisible => IsSelectionMode && SelectedItemIds.Any();
    public bool IsDeleteSelectedVisible => IsSelectionMode && SelectedItemIds.Any();

    public event EventHandler? ToolbarVisibilityChanged;

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        await LoadSortedItemsAsync(string.Empty, SortCriterion.Title,  SortDirection.Ascending, cancellationToken);
    }

    protected override async Task LoadSortedItemsAsync(string query, SortCriterion criterion, SortDirection direction, CancellationToken cancellationToken)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            List<BookEntity> books = await trashService.SearchSortedTrashBooksAsync(query, criterion, direction, cancellationToken);
            TrashBooks = new ObservableCollection<BookEntity>(books);
            OnToolbarVisibilityChanged();
        });
    }

    protected override void OnSearchQueryChangedCore(string value)
    {
        OnPropertyChanged(nameof(EmptyStateMessage));
    }

    [RelayCommand]
    private async Task RestoreBookAsync(BookEntity book)
    {
        await trashService.RestoreBookAsync(book.Id);
        TrashBooks.Remove(book);
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private async Task HardDeleteBookAsync(BookEntity book)
    {
        await trashService.HardDeleteBookAsync(book.Id);
        TrashBooks.Remove(book);
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private void ToggleSelection(BookEntity book)
    {
        if (SelectedItemIds.Contains(book.Id))
        {
            SelectedItemIds.Remove(book.Id);
        }
        else
        {
            SelectedItemIds.Add(book.Id);
        }

        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private async Task RestoreSelectedAsync()
    {
        foreach (Guid id in SelectedItemIds.ToList())
        {
            BookEntity? book = TrashBooks.FirstOrDefault(b => b.Id == id);
            if (book is not null)
            {
                await trashService.RestoreBookAsync(id);
                TrashBooks.Remove(book);
            }
        }

        SelectedItemIds.Clear();
        IsSelectionMode = false;
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        foreach (Guid id in SelectedItemIds.ToList())
        {
            BookEntity? book = TrashBooks.FirstOrDefault(b => b.Id == id);
            if (book is not null)
            {
                await trashService.HardDeleteBookAsync(id);
                TrashBooks.Remove(book);
            }
        }

        SelectedItemIds.Clear();
        IsSelectionMode = false;
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private async Task RestoreAllAsync()
    {
        int count = await trashService.RestoreAllAsync();
        TrashBooks.Clear();
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private async Task DeleteAllAsync()
    {
        int count = await trashService.DeleteAllAsync();
        TrashBooks.Clear();
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private void EnterSelectionMode(BookEntity book)
    {
        IsSelectionMode = true;
        SelectedItemIds.Add(book.Id);
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private void ExitSelectionMode()
    {
        IsSelectionMode = false;
        SelectedItemIds.Clear();
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private async Task NavigateToTrashDetailAsync(Guid bookId)
    {
        Dictionary<string, object> parameters = new()
        {
            ["BookId"] = bookId
        };

        await Shell.Current.GoToAsync(Routes.TrashBookDetailPage, parameters);
    }

    public override void OnNavigatingFrom()
    {
        IsSelectionMode = false;
        SelectedItemIds.Clear();
        OnToolbarVisibilityChanged();
    }

    private void OnToolbarVisibilityChanged()
    {
        ToolbarVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }
}
