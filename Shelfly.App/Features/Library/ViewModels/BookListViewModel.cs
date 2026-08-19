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
    private CancellationTokenSource? _debounceTokenSource;

    [ObservableProperty]
    public partial ObservableCollection<BookEntity> Books { get; set; } = [];

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial SortCriterion SortCriterion { get; set; } = SortCriterion.Title;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

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

    [RelayCommand]
    private async Task SearchAsync(string? query)
    {
        if (_debounceTokenSource != null)
        {
            await _debounceTokenSource.CancelAsync();
        }
        _debounceTokenSource = new();
        CancellationToken token = _debounceTokenSource.Token;

        await Task.Delay(500, token);
        if (token.IsCancellationRequested)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(query))
        {
            await RefreshBooksAsync();
            return;
        }

        IsLoading = true;
        try
        {
            List<BookEntity> books = await libraryService.SearchBooksAsync(query);
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
                string fileName = $"shelfly_{DateTime.UtcNow:yyyy-MM-dd_HH-mm-ss}.json";
                string documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string fullPath = Path.Combine(documentsFolder, fileName);

                await File.WriteAllTextAsync(fullPath, exportResult.Value);

                _ = Application.Current?.Windows[0]?.Page?.DisplayAlertAsync(
                    AppResources.BookListPageExportSuccessMessage,
                    $"File saved to: {fullPath}",
                    "OK");
            }
            else
            {
                LogManager.GetCurrentClassLogger().Warn("Export failed: {Error}", exportResult.Error);
                _ = Application.Current?.Windows[0]?.Page?.DisplayAlertAsync(
                    AppResources.BookListPageExportErrorMessage,
                    exportResult.Error ?? "Unknown error",
                    "OK");
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
