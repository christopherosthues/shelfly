using Shelfly.App.Bookmarks;

namespace Shelfly.App.Books;

public partial class BookDetailPage : ContentPage
{
    public BookDetailPage(BookDetailViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BookDetailViewModel vm)
        {
            await vm.LoadBookDetailsAsync();
        }
    }
}
