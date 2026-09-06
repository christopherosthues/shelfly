using Shelfly.App.Pages;

namespace Shelfly.App.Features.Library;

public partial class BookListPage : ShelflyContentPageBase
{
    public BookListPage(BookListViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
