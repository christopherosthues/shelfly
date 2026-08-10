namespace Shelfly.App.Authentication;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();

        BindingContext  = viewModel;
    }
}