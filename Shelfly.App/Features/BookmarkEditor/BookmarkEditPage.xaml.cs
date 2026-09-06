using Shelfly.App.Pages;

namespace Shelfly.App.Features.BookmarkEditor;

public partial class BookmarkEditPage : ShelflyContentPageBase
{
    public BookmarkEditPage(BookmarkEditViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
