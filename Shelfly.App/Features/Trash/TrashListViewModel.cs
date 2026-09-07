using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Enums;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;

namespace Shelfly.App.Features.Trash;

public partial class TrashListViewModel(TrashService trashService) : SortableListViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyStateMessage))]
    public partial ObservableCollection<BookEntity> TrashBooks { get; set; } = [];

    [ObservableProperty] public partial bool IsSelectionMode { get; set; }

    [ObservableProperty] public partial ObservableCollection<object> SelectedItems { get; set; } = [];

    public string EmptyStateMessage => TrashBooks.Count == 0
        ? (string.IsNullOrWhiteSpace(SearchQuery)
            ? AppResources.TrashListPageEmptyStateMessage
            : AppResources.TrashListPageSearchEmptyMessage)
        : string.Empty;

    public bool IsRestoreAllVisible => TrashBooks.Any();
    public bool IsDeleteAllVisible => TrashBooks.Any();
    public bool IsRestoreSelectedVisible => IsSelectionMode && SelectedItems.Any();
    public bool IsDeleteSelectedVisible => IsSelectionMode && SelectedItems.Any();

    public bool IsSelectAllVisible => SelectedItems.Count != TrashBooks.Count;
    public bool IsDeselectAllVisible => IsSelectionMode && SelectedItems.Any();

    public event EventHandler? ToolbarVisibilityChanged;

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        await LoadSortedItemsAsync(string.Empty, SortCriterion.Title, SortDirection.Ascending, cancellationToken);
    }

    protected override async Task LoadSortedItemsAsync(string query, SortCriterion criterion, SortDirection direction,
        CancellationToken cancellationToken)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            List<BookEntity> books =
                await trashService.SearchSortedTrashBooksAsync(query, criterion, direction, cancellationToken);
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
    private async Task RestoreSelectedAsync()
    {
        foreach (BookEntity book in SelectedItems.ToList())
        {
            await trashService.RestoreBookAsync(book.Id);
            TrashBooks.Remove(book);
        }

        SelectedItems.Clear();
        IsSelectionMode = false;
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        foreach (BookEntity book in SelectedItems.ToList())
        {
            await trashService.HardDeleteBookAsync(book.Id);
            TrashBooks.Remove(book);
        }

        SelectedItems.Clear();
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
    private async Task NavigateToTrashDetailAsync(BookEntity book)
    {
        Dictionary<string, object> parameters = new()
        {
            ["BookId"] = book.Id
        };

        await Shell.Current.GoToAsync(Routes.TrashBookDetailPage, parameters);
    }

    [RelayCommand]
    private void SelectAll()
    {
        HashSet<Guid> selectedIds = [.. SelectedItems.Cast<BookEntity>().Select(static book => book.Id)];
        List<BookEntity> unselectedBooks = [.. TrashBooks.Where(book => !selectedIds.Contains(book.Id))];

        foreach (BookEntity book in unselectedBooks)
        {
            SelectedItems.Add(book);
        }

        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private void DeselectAll()
    {
        SelectedItems.Clear();
        OnToolbarVisibilityChanged();
    }

    public override void OnNavigatingFrom()
    {
        IsSelectionMode = false;
        SelectedItems.Clear();
        OnToolbarVisibilityChanged();
    }

    private void OnToolbarVisibilityChanged()
    {
        ToolbarVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }
}