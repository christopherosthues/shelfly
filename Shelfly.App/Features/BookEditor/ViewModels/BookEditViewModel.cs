using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Library.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.Common;

namespace Shelfly.App.Features.BookEditor.ViewModels;

public partial class BookEditViewModel(LibraryService libraryService) : ObservableObject
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
    public partial string? TitleError { get; set; }

    [ObservableProperty]
    public partial string? AuthorError { get; set; }

    [ObservableProperty]
    public partial string? PublisherError { get; set; }

    [ObservableProperty]
    public partial string? ISBNError { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    public Guid BookId { get; private set; }
    public bool IsEditMode => BookId != Guid.Empty;

    public void LoadBook(BookEntity book)
    {
        BookId = book.Id;
        Title = book.Title;
        Author = book.Author;
        Publisher = book.Publisher;
        ISBN = book.ISBN;
        PublishDate = book.PublishDate;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ClearErrors();
        bool isValid = Validate();

        if (!isValid)
        {
            return;
        }

        IsLoading = true;
        try
        {
            Result<BookEntity> result = IsEditMode
                ? await libraryService.UpdateBookAsync(BookId, Title, Author, ISBN, Publisher, PublishDate)
                : await libraryService.AddBookAsync(Title, Author, ISBN, Publisher, PublishDate);

            if (result.IsSuccess)
            {
                Application.Current?.Dispatcher.DispatchAsync(async () =>
                {
                    await Shell.Current!.GoToAsync($"//{Routes.BookListPage}");
                    Title = string.Empty;
                    Author = string.Empty;
                    Publisher = string.Empty;
                    ISBN = string.Empty;
                    PublishDate = null;
                    BookId = Guid.Empty;
                });
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool Validate()
    {
        bool isValid = true;

        if (string.IsNullOrWhiteSpace(Title))
        {
            TitleError = AppResources.BookEditPageTitleEmptyError;
            isValid = false;
        }
        else if (Title.Length > 256)
        {
            TitleError = AppResources.BookEditPageTitleMaxLengthError;
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(Author))
        {
            AuthorError = AppResources.BookEditPageAuthorEmptyError;
            isValid = false;
        }
        else if (Author.Length > 256)
        {
            AuthorError = AppResources.BookEditPageAuthorMaxLengthError;
            isValid = false;
        }

        if (string.IsNullOrWhiteSpace(Publisher))
        {
            PublisherError = AppResources.BookEditPagePublisherEmptyError;
            isValid = false;
        }
        else if (Publisher.Length > 256)
        {
            PublisherError = AppResources.BookEditPagePublisherMaxLengthError;
            isValid = false;
        }

        if (!IsbnValidator.IsValid(ISBN))
        {
            ISBNError = AppResources.BookEditPageISBNFormatError;
            isValid = false;
        }

        return isValid;
    }

    private void ClearErrors()
    {
        TitleError = null;
        AuthorError = null;
        PublisherError = null;
        ISBNError = null;
    }
}
