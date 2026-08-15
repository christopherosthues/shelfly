using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Routing;
using Shelfly.App.Services;
using Shelfly.Common.DTOs;

namespace Shelfly.App.Books;

public partial class BooksViewModel(BookApiService bookApiService, BookmarkApiService bookmarkApiService) : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Book> _books = [];

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SelectedBookChanged))]
    private Book? _selectedBook;

    public bool SelectedBookChanged => SelectedBook is not null;

    static INavigation Navigation => Application.Current!.MainPage!.Navigation;
    static Page CurrentPage => Application.Current!.MainPage!;

    [RelayCommand]
    public async Task LoadBooksAsync()
    {
        IsLoading = true;
        try
        {
            List<Book> books = await bookApiService.GetBooksAsync();
            Books = new ObservableCollection<Book>(books);
        }
        catch (Exception ex)
        {
            await CurrentPage.DisplayAlert("Error", $"Failed to load books: {ex.Message}", "OK");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task NavigateToBookDetailAsync()
    {
        if (SelectedBook is not null)
        {
            await Shell.Current.GoToAsync(Routes.BookDetailPage + "?bookId=" + SelectedBook.Id);
        }
    }

    [RelayCommand]
    public async Task AddBookCommandAsync()
    {
        await Shell.Current.GoToAsync(Routes.AddEditBookPage);
    }
}