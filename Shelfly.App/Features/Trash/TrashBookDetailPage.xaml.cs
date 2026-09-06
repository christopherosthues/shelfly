using Shelfly.App.Pages;

namespace Shelfly.App.Features.Trash;

public partial class TrashBookDetailPage : ShelflyContentPageBase
{
    public TrashBookDetailPage(TrashBookDetailViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
