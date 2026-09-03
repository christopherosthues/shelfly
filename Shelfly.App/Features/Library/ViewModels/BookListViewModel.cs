using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Shelfly.App.Data.Entities;
using Shelfly.App.Enums;
using Shelfly.App.Features.BookEditor.ViewModels;
using Shelfly.App.Features.Library.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.App.Services;
using Shelfly.App.ViewModels;
using Shelfly.Common;

namespace Shelfly.App.Features.Library.ViewModels;

public partial class BookListViewModel(LibraryService libraryService, LibraryExportService exportService) : SortableListViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyStateMessage))]
    public partial ObservableCollection<BookEntity> Books { get; set; } = [];

    public string EmptyStateMessage => Books.Count == 0
        ? (string.IsNullOrWhiteSpace(SearchQuery)
            ? AppResources.BookListPageEmptyStateMessage
            : AppResources.BookListPageSearchEmptyMessage)
        : string.Empty;

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
        }
    }

    [RelayCommand]
    private static async Task NavigateToAddBookAsync()
    {
        await Shell.Current.GoToAsync(Routes.BookEditPage);
    }

    [RelayCommand]
    private static async Task NavigateToDetailBookAsync(Guid bookId)
    {
        await Shell.Current.GoToAsync(Routes.BookDetailPage, new Dictionary<string, object> { [nameof(BookDetailViewModel.BookId)] = bookId });
    }

    [RelayCommand]
    private static async Task NavigateToEditBookAsync(Guid bookId)
    {
        await Shell.Current.GoToAsync(Routes.BookEditPage, new Dictionary<string, object> { [nameof(BookEditViewModel.BookId)] = bookId });
    }

    [RelayCommand]
    private async Task ExportLibraryAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            Result<string> exportResult = await exportService.ExportLibraryToJsonAsync();

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

                    Page? page = Application.Current?.Windows[0].Page;
                    if (page is not null)
                    {
                        await page.DisplayAlertAsync(
                            AppResources.BookListPageExportSuccessMessage,
                            $"{AppResources.BookListPageExportFileSavedPrefix} {fullPath}",
                            AppResources.CommonOkButton);
                    }
                }
            }
            else
            {
                LogManager.GetCurrentClassLogger().Warn("Export failed: {Error}", exportResult.Error);
                Page? page = Application.Current?.Windows[0].Page;
                if (page is not null)
                {
                    await page.DisplayAlertAsync(
                        AppResources.BookListPageExportErrorMessage,
                        exportResult.Error ?? AppResources.BookListPageUnknownErrorMessage,
                        AppResources.CommonOkButton);
                }
            }
        });
    }
}
