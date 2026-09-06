using Shelfly.App.Pages;

namespace Shelfly.App.Features.BookEditor;

public partial class BookEditPage : ShelflyContentPageBase
{
    public BookEditPage(BookEditViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
