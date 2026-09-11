using Shelfly.App.Pages;

namespace Shelfly.App.Features.BookEditor;

public partial class BookEditPage : ShelflyContentPageBase
{
    public BookEditPage(BookEditViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        IServiceProvider? services = Handler?.MauiContext?.Services;
        if (services is null)
        {
            return;
        }

        if (Resources.TryGetValue("IsbnValidator", out object? resource)
            && resource is BookIsbnValidator isbnValidator)
        {
            isbnValidator.ServiceProvider = services;
        }
    }
}
