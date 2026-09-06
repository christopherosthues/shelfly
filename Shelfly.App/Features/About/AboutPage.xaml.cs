using Shelfly.App.Pages;

namespace Shelfly.App.Features.About;

public partial class AboutPage : ShelflyContentPageBase
{
    public AboutPage(AboutViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
