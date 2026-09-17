namespace Shelfly.App.Features.Settings;

public partial class SettingsPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}
