using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Routing;
using Shelfly.App.Services;
using Shelfly.Common.DTOs;

namespace Shelfly.App.Books;

[QueryProperty(nameof(BookId), "bookId")]
public partial class BookDetailViewModel(BookApiService bookApiService, BookmarkApiService bookmarkApiService)
    : ObservableObject
{
    [ObservableProperty]
    private Guid _bookId;

    [ObservableProperty]
    private Book? _book;

    [ObservableProperty]
    private ObservableCollection<Bookmark> _bookmarks = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _newBookmarkStartPage = string.Empty;

    [ObservableProperty]
    private string _newBookmarkEndPage = string.Empty;

    [ObservableProperty]
    private string _newBookmarkNote = string.Empty;

    [RelayCommand]
    private async Task LoadBookDetailsAsync()
    {
        IsLoading = true;
        try
        {
            Book? book = await bookApiService.GetBookAsync(_bookId);
            if (book is not null)
            {
                Book = book;
                Bookmarks = new ObservableCollection<Bookmark>(book.Bookmarks);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to load book details: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task AddBookmarkCommandAsync()
    {
        if (!int.TryParse((string?)NewBookmarkStartPage, out int startPage))
        {
            return;
        }

        int? endPage = null;
        if (NewBookmarkEndPage.Length > 0 && int.TryParse((string?)NewBookmarkEndPage, out int ep))
        {
            endPage = ep;
        }

        Bookmark bookmark = new()
        {
            StartPage = startPage,
            EndPage = endPage,
            Note = NewBookmarkNote
        };

        IsLoading = true;
        try
        {
            bool success = await bookmarkApiService.CreateBookmarkAsync(_bookId, bookmark);
            if (success)
            {
                Bookmarks.Add(bookmark);
                NewBookmarkStartPage = string.Empty;
                NewBookmarkEndPage = string.Empty;
                NewBookmarkNote = string.Empty;
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to add bookmark: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task EditBookAsync()
    {
        if (Book is not null)
        {
            await Shell.Current.GoToAsync(Routes.AddEditBookPage + "?bookId=" + Book.Id);
        }
    }

    [RelayCommand]
    private async Task DeleteBookAsync()
    {
        bool confirmed = await Shell.Current.DisplayAlertAsync("Delete Book", "Are you sure?", "Yes", "No");
        if (confirmed)
        {
            IsLoading = true;
            try
            {
                bool success = await bookApiService.DeleteBookAsync(_bookId);
                if (success)
                {
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to delete book: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task EditBookmarkAsync(Bookmark? bookmark)
    {
        if (bookmark is null)
        {
            return;
        }

        string? note = await Shell.Current.DisplayPromptAsync("Edit Bookmark", "Enter new note:", bookmark.Note, "Cancel");
        if (note is not null)
        {
            IsLoading = true;
            try
            {
                bookmark.Note = note;
                bool success = await bookmarkApiService.UpdateBookmarkAsync(bookmark);
                if (success)
                {
                    int index = Bookmarks.IndexOf(bookmark);
                    if (index >= 0)
                    {
                        Bookmarks[index] = bookmark;
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to update bookmark: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
    }

    [RelayCommand]
    private async Task DeleteBookmarkAsync(Bookmark? bookmark)
    {
        if (bookmark is null)
        {
            return;
        }

        IsLoading = true;
        try
        {
            bool success = await bookmarkApiService.DeleteBookmarkAsync(bookmark.Id);
            if (success)
            {
                Bookmarks.Remove(bookmark);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Error", $"Failed to delete bookmark: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }
}
