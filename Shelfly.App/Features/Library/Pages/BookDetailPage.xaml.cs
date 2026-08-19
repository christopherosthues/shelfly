using Shelfly.App.Features.Library.ViewModels;
using Shelfly.App.Pages;

namespace Shelfly.App.Features.Library.Pages;

public partial class BookDetailPage : ShelflyContentPageBase
{
    public BookDetailPage(BookDetailViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
