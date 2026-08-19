using Shelfly.App.Features.Library.ViewModels;

namespace Shelfly.App.Features.Library.Pages;

public partial class BookListPage : ContentPage
{
    public BookListViewModel ViewModel { get; }

    public BookListPage(BookListViewModel viewModel)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }
}
