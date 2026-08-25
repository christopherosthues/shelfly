using Shelfly.App.Features.BookEditor.ViewModels;
using Shelfly.App.Pages;

namespace Shelfly.App.Features.BookEditor.Pages;

public partial class BookEditPage : ShelflyContentPageBase
{
    public BookEditPage(BookEditViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
