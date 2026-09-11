using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Library;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;
using Shelfly.Common;

namespace Shelfly.App.Features.BookEditor;

public partial class BookEditViewModel(LibraryService libraryService) : ShelflyViewModelBase, IQueryAttributable
{
    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Author { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Publisher { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ISBN { get; set; } = string.Empty;

    [ObservableProperty]
    public partial DateTime? PublishDate { get; set; }

    [ObservableProperty]
    public partial bool IsTitleValid { get; set; } = true;

    [ObservableProperty]
    public partial bool IsAuthorValid { get; set; } = true;

    [ObservableProperty]
    public partial bool IsPublisherValid { get; set; } = true;

    [ObservableProperty]
    public partial bool IsIsbnValid { get; set; } = true;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    [ObservableProperty]
    public partial bool IsSaving { get; set; } = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageTitle))]
    public partial Guid BookId { get; private set; }

    private bool IsEditMode => BookId != Guid.Empty;

    public string PageTitle =>
        IsEditMode ? AppResources.BookEditPageEditBookTitle : AppResources.BookEditPageNewBookTitle;

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            if (BookId != Guid.Empty)
            {
                BookEntity? book = await libraryService.GetBookByIdAsync(BookId, cancellationToken);
                if (book is not null)
                {
                    Title = book.Title;
                    Author = book.Author;
                    Publisher = book.Publisher;
                    ISBN = book.ISBN;
                    PublishDate = book.PublishDate;
                }
                else
                {
                    await Shell.Current.DisplayAlertAsync(AppResources.BookEditPageBookNotFoundTitle,
                        AppResources.BookEditPageBookNotFoundMessage, AppResources.CommonOkButton);
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
        if (!query.TryGetValue(nameof(BookId), out var bookId) || bookId is not Guid id)
        {
            BookId = Guid.Empty;
            return;
        }

        BookId = id;
    }

    public override void OnNavigatingFrom()
    {
        base.OnNavigatingFrom();

        BookId = Guid.Empty;
        Title = string.Empty;
        Author = string.Empty;
        Publisher = string.Empty;
        ISBN = string.Empty;
        PublishDate = null;
        ClearErrors();

        if (SaveCommand.CanBeCanceled)
        {
            SaveCommand.Cancel();
        }
    }

    [RelayCommand]
    private void ClearPublishDate()
    {
        PublishDate = null;
    }

    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        ClearErrors();

        if (!(IsTitleValid && IsAuthorValid && IsPublisherValid && IsIsbnValid))
        {
            return;
        }

        IsSaving = true;
        try
        {
            Result<BookEntity> result = IsEditMode
                ? await libraryService.UpdateBookAsync(BookId, Title, Author, ISBN, Publisher, PublishDate, cancellationToken)
                : await libraryService.AddBookAsync(Title, Author, ISBN, Publisher, PublishDate, cancellationToken);

            if (result.IsSuccess)
            {
                await Shell.Current!.GoToAsync("..");
            }
            else if (result.Error?.Contains("ISBN", StringComparison.OrdinalIgnoreCase) == true)
            {
                // TODO: duplicated ISBN error handling
                // IsbnDuplicateError = AppResources.BookEditPageISBNDuplicateError;
            }
        }
        finally
        {
            IsSaving = false;
        }
    }

    private void ClearErrors()
    {
        IsTitleValid = true;
        IsAuthorValid = true;
        IsPublisherValid = true;
        IsIsbnValid = true;
    }
}
