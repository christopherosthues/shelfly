using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Trash.Services;
using Shelfly.App.Resources.Localization;
using Shelfly.App.ViewModels;

namespace Shelfly.App.Features.Trash.ViewModels;

public partial class TrashBookDetailViewModel(TrashService trashService) : ShelflyViewModelBase, IQueryAttributable
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
            List<BookEntity> books = await trashService.GetAllTrashBooksAsync(cancellationToken);
            Book = books.FirstOrDefault(b => b.Id == BookId);

            if (Book is not null)
            {
                Bookmarks = await trashService.GetBookmarksByBookIdAsync(BookId, cancellationToken);
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

    [RelayCommand]
    private async Task RestoreBookAsync(CancellationToken cancellationToken = default)
    {
        if (Book is null)
        {
            return;
        }

        await trashService.RestoreBookAsync(Book.Id, cancellationToken);

        // Navigate back to trash list
        await Shell.Current.GoToAsync($"//{Routes.TrashListPage}");
    }

    [RelayCommand]
    private async Task HardDeleteBookAsync(CancellationToken cancellationToken = default)
    {
        if (Book is null)
        {
            return;
        }

        BookEntity? book = await trashService.HardDeleteBookAsync(Book.Id, cancellationToken);

        if (book is not null)
        {
            // Navigate back to trash list
            await Shell.Current.GoToAsync($"//{Routes.TrashListPage}");
        }
    }

    [RelayCommand]
    private async Task ShowNoteAsync(string? note, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return;
        }

        await Shell.Current.DisplayAlertAsync(AppResources.BookDetailPageNoteAlertTitle, note, AppResources.CommonOkButton);
    }
}
