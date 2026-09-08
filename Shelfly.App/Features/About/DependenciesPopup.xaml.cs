using CommunityToolkit.Maui.Extensions;

namespace Shelfly.App.Features.About;

public partial class DependenciesPopup : ContentView
{
    public DependenciesPopup(AboutViewModel viewModel)
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
