using Shelfly.App.Features.Library.ViewModels;
using Shelfly.App.Pages;

namespace Shelfly.App.Features.Library.Pages;

public partial class BookListPage : ShelflyContentPageBase
{
    private BookListViewModel? ViewModel =>  BindingContext as BookListViewModel;

    public BookListPage(BookListViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }


}
