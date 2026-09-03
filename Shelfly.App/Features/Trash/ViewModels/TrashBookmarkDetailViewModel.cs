using CommunityToolkit.Mvvm.ComponentModel;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.Trash.Services;
using Shelfly.App.ViewModels;

namespace Shelfly.App.Features.Trash.ViewModels;

public partial class TrashBookmarkDetailViewModel(TrashService trashService) : ShelflyViewModelBase, IQueryAttributable
{
    [ObservableProperty]
    public partial Guid BookmarkId { get; set; } = Guid.Empty;

    [ObservableProperty]
    public partial string? Note { get; set; }

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = true;

    protected override async Task LoadAsync(CancellationToken cancellationToken)
    {
        IsLoading = true;
        try
        {
            // Load bookmark note from database via service
            BookmarkEntity? bookmark = await trashService.GetBookmarkByIdAsync(BookmarkId, cancellationToken);
            
            Note = bookmark?.Note;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (!query.TryGetValue(nameof(BookmarkId), out var bookmarkId) || bookmarkId is not Guid id)
        {
            BookmarkId = Guid.Empty;
            return;
        }

        BookmarkId = id;
    }
}
