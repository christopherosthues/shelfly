using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Library.Services;

namespace Shelfly.App.Features.Library.ViewModels;

public partial class BookListViewModel : ObservableObject
{
    private readonly LibraryService _libraryService;
    private CancellationTokenSource? _debounceTokenSource;

    [ObservableProperty]
    public partial ObservableCollection<BookEntity> Books { get; set; } = [];

    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial SortCriterion SortCriterion { get; set; } = SortCriterion.Title;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    public List<SortCriterion> SortOptions { get; } =
    [
        SortCriterion.Title,
        SortCriterion.Author,
        SortCriterion.Publisher,
        SortCriterion.PublishDate
    ];

    public BookListViewModel(LibraryService libraryService)
    {
        _libraryService = libraryService;

        LoadBooksAsync().ContinueWith(_ => IsLoading = false, TaskContinuationOptions.OnlyOnRanToCompletion);
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
            List<BookEntity> books = await _libraryService.SearchBooksAsync(query);
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
        BookEntity? book = await _libraryService.SoftDeleteBookAsync(bookId);
        if (book is not null)
        {
            Books.Remove(book);
        }
    }

    [RelayCommand]
    private static async Task NavigateToAddBookAsync()
    {
        await Shell.Current!.GoToAsync($"//{Routes.BookEditPage}");
    }

    [RelayCommand]
    private static async Task NavigateToEditBookAsync(Guid bookId)
    {
        await Shell.Current!.GoToAsync($"{Routes.BookEditPage}/{bookId}", new Dictionary<string, object> { ["bookId"] = bookId });
    }

    private async Task LoadBooksAsync()
    {
        IsLoading = true;
        try
        {
            List<BookEntity> books = await _libraryService.SortBooksAsync(SortCriterion.Title);
            Books = new ObservableCollection<BookEntity>(books);
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
            List<BookEntity> books = await _libraryService.SortBooksAsync(SortCriterion);
            Books = new ObservableCollection<BookEntity>(books);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
