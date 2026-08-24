using Shelfly.App.Features.Library.ViewModels;
using Shelfly.App.Pages;

namespace Shelfly.App.Features.Library.Pages;

public partial class BookListPage : ShelflyContentPageBase
{
    public BookListPage(BookListViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
