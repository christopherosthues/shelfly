namespace Shelfly.App.Features.Settings;

public partial class ServerEntryPage
{
    public ServerEntryPage(ServerEntryViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
