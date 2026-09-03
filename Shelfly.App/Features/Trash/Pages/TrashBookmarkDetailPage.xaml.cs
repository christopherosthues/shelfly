using Shelfly.App.Features.Trash.ViewModels;
using Shelfly.App.Pages;

namespace Shelfly.App.Features.Trash.Pages;

public partial class TrashBookmarkDetailPage : ShelflyContentPageBase
{
    public TrashBookmarkDetailPage(TrashBookmarkDetailViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
