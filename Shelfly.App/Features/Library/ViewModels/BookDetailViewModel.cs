using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.BookmarkEditor.ViewModels;
using Shelfly.App.Features.Library.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;
using Shelfly.Common;

namespace Shelfly.App.Features.Library.ViewModels;

public partial class BookDetailViewModel(LibraryService libraryService) : ShelflyViewModelBase, IQueryAttributable
{
    [ObservableProperty]
    public partial Guid BookId { get; set; } = Guid.Empty;

    [ObservableProperty]
    public partial BookEntity? Book { get; set; }

    [ObservableProperty]
    public partial List<BookmarkEntity> Bookmarks { get; set; } = [];

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            Book = await libraryService.GetBookByIdAsync(BookId, cancellationToken);
            Bookmarks = await libraryService.GetBookmarksByBookIdAsync(BookId, cancellationToken);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task DeleteBookAsync(CancellationToken cancellationToken = default)
    {
        if (Book is null)
        {
            return;
        }

        Result<bool> result = await libraryService.SoftDeleteBookWithBookmarksAsync(Book.Id, cancellationToken);

        if (result.IsSuccess)
        {
            Book = null;
            Bookmarks = [];
        }
    }

    [RelayCommand]
    private async Task AddBookmarkAsync(CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> parameters = new()
        {
            [nameof(BookmarkEditViewModel.BookId)] = Book?.Id ?? Guid.Empty,
            [nameof(BookmarkEditViewModel.BookmarkId)] = Guid.Empty
        };

        await Shell.Current.GoToAsync(Routes.BookmarkEditPage, parameters);
    }

    [RelayCommand]
    private async Task DeleteBookmarkAsync(BookmarkEntity bookmark, CancellationToken cancellationToken = default)
    {
        Result<BookmarkEntity?> result = await libraryService.DeleteBookmarkAsync(bookmark.Id, cancellationToken);

        if (result.IsSuccess)
        {
            Bookmarks.Remove(bookmark);
        }
    }

    [RelayCommand]
    private async Task ShowNoteAsync(string? note, CancellationToken cancellationToken = default)
    {
        await Shell.Current.DisplayAlertAsync(AppResources.BookDetailPageNoteAlertTitle, note, "OK");
    }

    [RelayCommand]
    private async Task EditBookmarkAsync(BookmarkEntity? bookmark, CancellationToken cancellationToken = default)
    {
        Dictionary<string, object> parameters = new()
        {
            [nameof(BookmarkEditViewModel.BookId)] = Book?.Id ?? Guid.Empty,
            [nameof(BookmarkEditViewModel.BookmarkId)] = bookmark?.Id ?? Guid.Empty
        };

        await Shell.Current.GoToAsync(Routes.BookmarkEditPage, parameters);
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue(nameof(BookId), out var bookId) || bookId is not Guid id)
        {
            return;
        }

        BookId = id;
    }
}
