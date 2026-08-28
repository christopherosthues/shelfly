using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NLog;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.BookEditor.ViewModels;
using Shelfly.App.Features.Library.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.App.Services;
using Shelfly.App.ViewModels;
using Shelfly.Common;

namespace Shelfly.App.Features.Library.ViewModels;

public partial class BookListViewModel(LibraryService libraryService, LibraryExportService exportService) : ShelflyViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmptyStateMessage))]
    public partial ObservableCollection<BookEntity> Books { get; set; } = [];

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial SortCriterion SortCriterion { get; set; } = SortCriterion.Title;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    public string EmptyStateMessage => Books.Count == 0
        ? (string.IsNullOrWhiteSpace(SearchQuery)
            ? AppResources.BookListPageEmptyStateMessage
            : AppResources.BookListPageSearchEmptyMessage)
        : string.Empty;

    public List<SortCriterion> SortOptions { get; } = [.. Enum.GetValues<SortCriterion>()];

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            List<BookEntity> books = await libraryService.SortBooksAsync(SortCriterion.Title, cancellationToken);
            Books = new ObservableCollection<BookEntity>(books);
        }
        finally
        {
            IsLoading = false;
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        SearchCommand.Execute(null);
        OnPropertyChanged(nameof(EmptyStateMessage));
    }

    [RelayCommand]
    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            await RefreshBooksAsync();
            return;
        }

        IsLoading = true;
        try
        {
            List<BookEntity> books = await libraryService.SearchBooksAsync(SearchQuery, cancellationToken);
            Books = new ObservableCollection<BookEntity>(books);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SortAsync(SortCriterion criterion)
    {
        SortCriterion = criterion;
        await RefreshBooksAsync();
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
        IsLoading = true;
        try
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
                // DefaultFileName = $"shelfly_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.json",

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
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task RefreshBooksAsync()
    {
        IsLoading = true;
        try
        {
            List<BookEntity> books = await libraryService.SortBooksAsync(SortCriterion);
            Books = new ObservableCollection<BookEntity>(books);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
