using Shelfly.App.Bookmarks;

namespace Shelfly.App.Books;

public partial class AddEditBookPage : ContentPage
{
    public AddEditBookPage(AddEditBookViewModel viewModel)
    {
        InitializeComponent();

        BindingContext = viewModel;
    }
}
