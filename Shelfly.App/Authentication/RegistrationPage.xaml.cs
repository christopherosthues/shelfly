namespace Shelfly.App.Authentication;

public partial class RegistrationPage : ContentPage
{
    public RegistrationPage(RegistrationViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}