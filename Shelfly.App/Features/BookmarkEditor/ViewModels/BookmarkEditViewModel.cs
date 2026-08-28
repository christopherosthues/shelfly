using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Library.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;
using Shelfly.Common;

namespace Shelfly.App.Features.BookmarkEditor.ViewModels;

public partial class BookmarkEditViewModel(LibraryService libraryService)
    : ShelflyViewModelBase, IQueryAttributable
{
    [ObservableProperty]
    public partial int StartPage { get; set; } = 1;

    [ObservableProperty]
    public partial int? EndPage { get; set; }

    [ObservableProperty]
    public partial string? Note { get; set; }

    [ObservableProperty]
    public partial string? StartPageError { get; set; }

    [ObservableProperty]
    public partial string? EndPageError { get; set; }

    [ObservableProperty]
    public partial string? NoteError { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    [ObservableProperty]
    public partial bool IsSaving { get; set; } = false;

    private bool IsEditMode => BookmarkId != Guid.Empty;

    public string PageTitle => IsEditMode
        ? AppResources.BookmarkEditPageEditBookmarkTitle
        : AppResources.BookmarkEditPageNewBookmarkTitle;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    public partial Guid BookmarkId { get; set; }

    public Guid BookId { get; set; }

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            if (BookmarkId != Guid.Empty)
            {
                BookmarkEntity? bookmark = await libraryService.GetBookmarkByIdAsync(BookmarkId, cancellationToken);
                if (bookmark is not null)
                {
                    StartPage = bookmark.StartPage;
                    EndPage = bookmark.EndPage;
                    Note = bookmark.Note;
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(AppResources.BookmarkEditPageBookmarkNotFoundTitle,
                        AppResources.BookmarkEditPageBookmarkNotFoundMessage, AppResources.CommonOkButton);
                    await Shell.Current.GoToAsync("..");
                }
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue(nameof(BookId), out var bookId) || bookId is not Guid bookGuid ||
            !query.TryGetValue(nameof(BookmarkId), out var bookmarkId) || bookmarkId is not Guid bookmarkGuid)
        {
            return;
        }

        BookId = bookGuid;
        BookmarkId = bookmarkGuid;
    }

    public override void OnNavigatingFrom()
    {
        base.OnNavigatingFrom();

        BookId = Guid.Empty;
        BookmarkId = Guid.Empty;
        StartPage = 1;
        EndPage = null;
        Note = null;
        StartPageError = null;
        EndPageError = null;

        if (SaveCommand.CanBeCanceled)
        {
            SaveCommand.Cancel();
        }
    }

    partial void OnStartPageChanged(int value)
    {
        ValidatePages();
    }

    partial void OnEndPageChanged(int? value)
    {
        ValidatePages();
    }

    partial void OnNoteChanged(string? value)
    {
        NoteError = value is not null && value.Length > 1000 ? AppResources.BookmarkEditPageNoteMaxLengthError : null;
    }

    private void ValidatePages()
    {
        StartPageError = StartPage <= 0 ? AppResources.BookmarkEditPageStartPageEmptyError : null;
        EndPageError = EndPage.HasValue && EndPage.Value < StartPage ? AppResources.BookmarkEditPageEndPageRangeError : null;
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        ValidatePages();

        if (StartPageError is not null || EndPageError is not null || NoteError is not null)
        {
            return;
        }

        IsSaving = true;
        try
        {
            Result<BookmarkEntity> result = IsEditMode
                ? await libraryService.UpdateBookmarkAsync(BookmarkId, StartPage, EndPage, Note, cancellationToken)
                : await libraryService.AddBookmarkAsync(BookId, StartPage, EndPage, Note, cancellationToken);

            if (result.IsSuccess)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlertAsync(AppResources.BookmarkEditPageSaveFailedTitle, AppResources.BookmarkEditPageSaveFailedMessage, AppResources.CommonOkButton);
            }
        }
        finally
        {
            IsSaving = false;
        }
    }
}
