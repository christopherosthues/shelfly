using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Services;
using Shelfly.Common.DTOs;

namespace Shelfly.App.Books;

[QueryProperty(nameof(BookId), "bookId")]
public partial class AddEditBookViewModel(BookApiService bookApiService) : ObservableObject, IQueryAttributable
{
    private readonly Book? _editBook;

    [ObservableProperty]
    private Guid? _bookId;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _author = string.Empty;

    [ObservableProperty]
    private string _isbn = string.Empty;

    [ObservableProperty]
    private DateTime _publishDate = DateTime.Today;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _validationError_Title;

    [ObservableProperty]
    private string? _validationError_ISBN;

    [ObservableProperty]
    private string? _validationError_PublishDate;

    public string PageTitle => _editBook is null ? "Add Book" : "Edit Book";
    public string SubmitButtonText => _editBook is null ? "Add" : "Update";

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Title))
        {
            ValidationError_Title = "Title is required";
            return;
        }

        ValidationError_Title = null;
        ValidationError_ISBN = null;
        ValidationError_PublishDate = null;

        Book book = new()
        {
            Title = Title,
            Author = Author,
            ISBN = Isbn,
            PublishDate = PublishDate
        };

        IsLoading = true;
        try
        {
            bool success;
            if (_editBook is not null)
            {
                book.Id = _editBook.Id;
                success = await bookApiService.UpdateBookAsync(book);
            }
            else
            {
                success = await bookApiService.CreateBookAsync(book);
            }

            if (success)
            {
                await Shell.Current.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            string action = _editBook is null ? "add" : "update";
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to {action} book: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        // TODO: load book if not null
        // _editBook = editBook;
        //
        // if (editBook is not null)
        // {
        //     Title = editBook.Title;
        //     Author = editBook.Author;
        //     Isbn = editBook.ISBN;
        //     PublishDate = editBook.PublishDate;
        // }
    }
}
