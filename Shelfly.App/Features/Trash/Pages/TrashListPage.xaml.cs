using Shelfly.App.Features.Trash.ViewModels;
using Shelfly.App.Pages;

namespace Shelfly.App.Features.Trash.Pages;

public partial class TrashListPage : ShelflyContentPageBase
{
    public TrashListPage(TrashListViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
