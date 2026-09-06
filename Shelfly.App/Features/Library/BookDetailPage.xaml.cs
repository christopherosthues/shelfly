using Shelfly.App.Pages;

namespace Shelfly.App.Features.Library;

public partial class BookDetailPage : ShelflyContentPageBase
{
    public BookDetailPage(BookDetailViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
