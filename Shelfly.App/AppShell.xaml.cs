using Shelfly.App.Features.BookEditor.Pages;
using Shelfly.App.Features.BookmarkEditor.Pages;
using Shelfly.App.Features.Library.Pages;

namespace Shelfly.App;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();

        Routing.RegisterRoute(typeof(BookEditPage).FullName!);
        Routing.RegisterRoute(typeof(BookDetailPage).FullName!);
        Routing.RegisterRoute(typeof(BookmarkEditPage).FullName!);
    }
}