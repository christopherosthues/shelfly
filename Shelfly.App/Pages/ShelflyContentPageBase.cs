using Shelfly.App.ViewModels;

namespace Shelfly.App.Pages;

public partial class ShelflyContentPageBase : ContentPage
{
    private ShelflyViewModelBase? ViewModel =>  BindingContext as ShelflyViewModelBase;

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);

        if (ViewModel != null)
        {
            await ViewModel.OnNavigatedToAsync();
        }
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        ViewModel?.OnNavigatingFrom();

        base.OnNavigatedFrom(args);
    }
}