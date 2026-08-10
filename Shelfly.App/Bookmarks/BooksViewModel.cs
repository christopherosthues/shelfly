using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Shelfly.Common;

namespace Shelfly.App.Bookmarks;

public partial class BooksViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Book> _books = [];
}