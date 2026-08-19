using Microsoft.EntityFrameworkCore;
using Shelfly.App.Data;
using Shelfly.App.Data.Entities;
using Shelfly.App.Features.BookEditor.ViewModels;

namespace Shelfly.App.Features.BookEditor.Pages;

public partial class BookEditPage : ContentPage
{
    public BookEditViewModel ViewModel { get; }

    public BookEditPage(BookEditViewModel viewModel, LocalDbContext dbContext)
    {
        ViewModel = viewModel;
        BindingContext = viewModel;
        InitializeComponent();
    }
}
