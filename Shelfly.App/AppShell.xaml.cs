namespace Shelfly.App;

public partial class AppShell : Shell
{
    public AppShell(AppShellViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
}