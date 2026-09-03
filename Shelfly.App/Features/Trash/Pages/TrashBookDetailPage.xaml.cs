using Shelfly.App.Features.Trash.ViewModels;
using Shelfly.App.Pages;

namespace Shelfly.App.Features.Trash.Pages;

public partial class TrashBookDetailPage : ShelflyContentPageBase
{
    public TrashBookDetailPage(TrashBookDetailViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
