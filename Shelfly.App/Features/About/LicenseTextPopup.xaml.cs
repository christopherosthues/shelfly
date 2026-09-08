using CommunityToolkit.Maui.Extensions;

namespace Shelfly.App.Features.About;

public partial class LicenseTextPopup : ContentView
{
    public LicenseTextPopup(AboutViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }

    private async void OnCloseButtonClicked(object? sender, EventArgs e)
    {
        if (Shell.Current is not null)
        {
            await Shell.Current.ClosePopupAsync();
        }
    }
}
