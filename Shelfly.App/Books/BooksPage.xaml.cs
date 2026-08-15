namespace Shelfly.App.Books;

public partial class BooksPage : ContentPage
{
    public BooksPage(BooksViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is BooksViewModel vm)
        {
            await vm.LoadBooksAsync();
        }
    }
}
