using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Shelfly.App.Data.Entities;
using Shelfly.App.Enums;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;
using Shelfly.Common;
using BookEditViewModel = Shelfly.App.Features.BookEditor.BookEditViewModel;

namespace Shelfly.App.Features.Library;

public partial class BookListViewModel(LibraryService libraryService, LibraryExportService exportService) : SortableListViewModelBase
{
    [ObservableProperty]
    public partial bool IsSelectionMode { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<object> SelectedItems { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyStateMessage))]
    public partial ObservableCollection<BookEntity> Books { get; set; } = [];

    public string EmptyStateMessage => Books.Count == 0
        ? string.IsNullOrWhiteSpace(SearchQuery)
            ? AppResources.BookListPageEmptyStateMessage
            : AppResources.BookListPageSearchEmptyMessage
        : string.Empty;

    public bool IsExportAllVisible => true;
    public bool IsSelectAllVisible => SelectedItems.Count != Books.Count;
    public bool IsDeleteSelectedVisible => IsSelectionMode && SelectedItems.Any();
    public bool IsExportSelectedVisible => IsSelectionMode && SelectedItems.Any();
    public bool IsDeselectAllVisible => IsSelectionMode && SelectedItems.Any();

    public event EventHandler? ToolbarVisibilityChanged;

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        await LoadSortedItemsAsync(string.Empty, SortCriterion.Title, SortDirection.Ascending, cancellationToken);
    }

    protected override async Task LoadSortedItemsAsync(string query, SortCriterion criterion, SortDirection direction, CancellationToken cancellationToken)
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            List<BookEntity> books = await libraryService.SearchSortedBooksAsync(query, criterion, direction, cancellationToken);
            Books = new ObservableCollection<BookEntity>(books);
            OnToolbarVisibilityChanged();
        });
    }

    protected override void OnSearchQueryChangedCore(string value)
    {
        OnPropertyChanged(nameof(EmptyStateMessage));
    }

    [RelayCommand]
    private async Task SoftDeleteAsync(Guid bookId)
    {
        BookEntity? book = await libraryService.SoftDeleteBookAsync(bookId);
        if (book is not null)
        {
            Books.Remove(book);
            OnToolbarVisibilityChanged();
        }
    }

    [RelayCommand]
    private async Task DeleteSelectedAsync()
    {
        List<BookEntity> booksToDelete = SelectedItems.Cast<BookEntity>().ToList();

        foreach (BookEntity book in booksToDelete)
        {
            BookEntity? deletedBook = await libraryService.SoftDeleteBookAsync(book.Id);
            if (deletedBook is not null)
            {
                Books.Remove(deletedBook);
            }
        }

        SelectedItems.Clear();
        IsSelectionMode = false;
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private static async Task NavigateToAddBookAsync()
    {
        await Shell.Current.GoToAsync(Routes.BookEditPage);
    }

    [RelayCommand]
    private static async Task NavigateToDetailBookAsync(BookEntity book)
    {
        await Shell.Current.GoToAsync(Routes.BookDetailPage, new Dictionary<string, object> { [nameof(BookDetailViewModel.BookId)] = book.Id });
    }

    [RelayCommand]
    private static async Task NavigateToEditBookAsync(Guid bookId)
    {
        await Shell.Current.GoToAsync(Routes.BookEditPage, new Dictionary<string, object> { [nameof(BookEditViewModel.BookId)] = bookId });
    }

    public override void OnNavigatingFrom()
    {
        IsSelectionMode = false;
        SelectedItems.Clear();
        OnToolbarVisibilityChanged();
    }

    [RelayCommand]
    private async Task ExportLibraryAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            Result<string> exportResult = await exportService.ExportLibraryToJsonAsync();

            await ExportBooksAsync(exportResult);
        });
    }

    private static async Task ExportBooksAsync(Result<string> exportResult)
    {
        if (exportResult.IsSuccess)
        {
            FilePickerFileType customFileType = new FilePickerFileType(
                new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, [".json"] },
                    { DevicePlatform.Android, [".json"] },
                    { DevicePlatform.WinUI, [".json"] },
                    { DevicePlatform.Tizen, [".json"] },
                    { DevicePlatform.macOS, [".json"] },
                });
            FileResult? fileResult = await FilePicker.Default.PickAsync(new PickOptions()
            {
                PickerTitle = AppResources.BookListPageExportLibraryButtonText,
                FileTypes = customFileType
            });

            if (fileResult is not null)
            {
                string fullPath = fileResult.FullPath;
                await File.WriteAllTextAsync(fullPath, exportResult.Value);

                await Shell.Current.DisplayAlertAsync(
                    AppResources.BookListPageExportSuccessMessage,
                    $"{AppResources.BookListPageExportFileSavedPrefix} {fullPath}",
                    AppResources.CommonOkButton);
            }
        }
        else
        {
            LogManager.GetCurrentClassLogger().Warn("Export failed: {Error}", exportResult.Error);
            await Shell.Current.DisplayAlertAsync(
                AppResources.BookListPageExportErrorMessage,
                exportResult.Error ?? AppResources.BookListPageUnknownErrorMessage,
                AppResources.CommonOkButton);
        }
    }

    [RelayCommand]
    private async Task ExportSelectedAsync()
    {
        List<BookEntity> selectedBooks = [.. SelectedItems.Cast<BookEntity>()];

        await ExecuteWithLoadingAsync(async () =>
        {
            Result<string> exportResult = await exportService.ExportSelectedBooksToJsonAsync(selectedBooks);

            await ExportBooksAsync(exportResult);
        });
    }

    [RelayCommand]
    private void SelectAll()
    {
        HashSet<Guid> selectedIds = [.. SelectedItems.Cast<BookEntity>().Select(static book => book.Id)];
        List<BookEntity> unselectedBooks = [.. Books.Where(book => !selectedIds.Contains(book.Id))];

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

    private void OnToolbarVisibilityChanged()
    {
        ToolbarVisibilityChanged?.Invoke(this, EventArgs.Empty);
    }
}
