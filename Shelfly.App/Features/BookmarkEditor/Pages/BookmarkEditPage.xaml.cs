using Shelfly.App.Features.BookmarkEditor.ViewModels;
using Shelfly.App.Pages;

namespace Shelfly.App.Features.BookmarkEditor.Pages;

public partial class BookmarkEditPage : ShelflyContentPageBase
{
    public BookmarkEditPage(BookmarkEditViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
