using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Library.Services;
using Shelfly.App.Features.Library.ViewModels;
using Shelfly.App.Features.Trash.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;

namespace Shelfly.App.Features.Trash.ViewModels;

public partial class TrashListViewModel(TrashService trashService) : ShelflyViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyStateMessage))]
    public partial ObservableCollection<BookEntity> TrashBooks { get; set; } = [];

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int SelectedSortOptionIndex { get; set; } = 0;

    [ObservableProperty]
    public partial SortDirection CurrentSortDirection { get; set; } = SortDirection.Ascending;

    [ObservableProperty]
    public partial bool IsSelectionMode { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<Guid> SelectedItemIds { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    public List<SortOptionDisplay> SortOptions { get; } =
    [
        new SortOptionDisplay(SortCriterion.Title, AppResources.BookListPageSortByTitle),
        new SortOptionDisplay(SortCriterion.Author, AppResources.BookListPageSortByAuthor),
        new SortOptionDisplay(SortCriterion.Publisher, AppResources.BookListPageSortByPublisher),
        new SortOptionDisplay(SortCriterion.PublishDate, AppResources.BookListPageSortByPublishDate)
    ];

    public string EmptyStateMessage => TrashBooks.Count == 0
        ? (string.IsNullOrWhiteSpace(SearchQuery)
            ? AppResources.TrashListPageEmptyStateMessage
            : AppResources.TrashListPageSearchEmptyMessage)
        : string.Empty;

    public bool IsRestoreAllVisible => !IsSelectionMode && TrashBooks.Any();
    public bool IsDeleteAllVisible => !IsSelectionMode && TrashBooks.Any();
    public bool IsRestoreSelectedVisible => IsSelectionMode && SelectedItemIds.Any();
    public bool IsDeleteSelectedVisible => IsSelectionMode && SelectedItemIds.Any();

    public string SortDirectionDescription => CurrentSortDirection == SortDirection.Ascending 
        ? AppResources.SortDirectionAscending 
        : AppResources.SortDirectionDescending;

    public string SortIconSource => CurrentSortDirection == SortDirection.Ascending 
        ? "sort_asc.svg" 
        : "sort_desc.svg";

    public event EventHandler? ToolbarVisibilityChanged;

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        List<BookEntity> books = await trashService.GetSortedTrashBooksAsync(
            SortOptions[SelectedSortOptionIndex].Criterion,
            CurrentSortDirection,
            cancellationToken);
        
        TrashBooks.Clear();

        foreach (BookEntity book in books)
        {
            TrashBooks.Add(book);
        }

        OnToolbarVisibilityChanged();
    }

    private async Task LoadSearchResultsAsync()
    {
        List<BookEntity> books = await trashService.SearchSortedTrashBooksAsync(
            SearchQuery,
            SortOptions[SelectedSortOptionIndex].Criterion,
            CurrentSortDirection);
        
        TrashBooks.Clear();

        foreach (BookEntity book in books)
        {
            TrashBooks.Add(book);
        }

        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadSearchResultsAsync();
    }

    [RelayCommand]
    private async Task SortAsync(SortOptionDisplay? selectedOption)
    {
        if (selectedOption is not null)
        {
            SelectedSortOptionIndex = SortOptions.IndexOf(selectedOption);
        }

        await LoadSearchResultsAsync();
    }

    [RelayCommand]
    private async Task ToggleSortDirectionAsync()
    {
        CurrentSortDirection = CurrentSortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
        
        await LoadSearchResultsAsync();
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
