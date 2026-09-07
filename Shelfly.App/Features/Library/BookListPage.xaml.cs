using Shelfly.App.Controls;
using Shelfly.App.Pages;
using Shelfly.App.Resources.Localization;

namespace Shelfly.App.Features.Library;

public partial class BookListPage : ShelflyContentPageBase
{
    private ToolbarItem? _exportAllItem;
    private ToolbarItem? _deleteSelectedItem;
    private ToolbarItem? _exportSelectedItem;
    private ToolbarItem? _selectAllItem;
    private ToolbarItem? _deselectAllItem;

    private BookListViewModel ViewModel => (BookListViewModel) BindingContext;

    public BookListPage(BookListViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();

        CreateToolbarItems(viewModel);
        viewModel.ToolbarVisibilityChanged += OnToolbarVisibilityChanged;
        MultiSelectView.SelectionChanged += OnSelectionChanged;
    }

    private void CreateToolbarItems(BookListViewModel viewModel)
    {
        _exportAllItem = new ToolbarItem
        {
            Text = AppResources.BookListPageExportLibraryButtonText,
            IconImageSource = "export_icon.svg",
            Command = viewModel.ExportLibraryCommand,
            Order = ToolbarItemOrder.Primary
        };
        SemanticProperties.SetDescription(_exportAllItem, AppResources.BookListPageExportLibraryDescription);

        _selectAllItem = new ToolbarItem
        {
            Text = AppResources.BookListPageSelectAllButtonText,
            IconImageSource = "select_all_icon.svg",
            Command = viewModel.SelectAllCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_selectAllItem, AppResources.BookListPageSelectAllDescription);

        _deleteSelectedItem = new ToolbarItem
        {
            Text = AppResources.BookListPageDeleteSelectedButtonText,
            IconImageSource = "delete_icon.svg",
            Command = viewModel.DeleteSelectedCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_deleteSelectedItem, AppResources.BookListPageDeleteSelectedDescription);

        _exportSelectedItem = new ToolbarItem
        {
            Text = AppResources.BookListPageExportSelectedButtonText,
            IconImageSource = "export_icon.svg",
            Command = viewModel.ExportSelectedCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_exportSelectedItem, AppResources.BookListPageExportSelectedDescription);

        _deselectAllItem = new ToolbarItem
        {
            Text = AppResources.BookListPageDeselectAllButtonText,
            IconImageSource = "deselect_all_icon.svg",
            Command = viewModel.DeselectAllCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_selectAllItem, AppResources.BookListPageDeselectAllDescription);
    }

    private void OnToolbarVisibilityChanged(object? sender, EventArgs e)
    {
        UpdateToolbarItems();
    }

    private void OnSelectionChanged(object? sender, MultiSelectSelectionChangedEventArgs e)
    {
        UpdateToolbarItems();
    }

    private void UpdateToolbarItems()
    {
        BookListViewModel viewModel = ViewModel;
        ToolbarItems.Clear();

        if (viewModel.IsExportAllVisible && _exportAllItem is not null)
        {
            ToolbarItems.Add(_exportAllItem);
        }

        if (viewModel.IsSelectAllVisible && _selectAllItem is not null)
        {
            ToolbarItems.Add(_selectAllItem);
        }

        if (viewModel.IsDeleteSelectedVisible && _deleteSelectedItem is not null)
        {
            ToolbarItems.Add(_deleteSelectedItem);
        }

        if (viewModel.IsExportSelectedVisible && _exportSelectedItem is not null)
        {
            ToolbarItems.Add(_exportSelectedItem);
        }

        if (viewModel.IsDeselectAllVisible && _deselectAllItem is not null)
        {
            ToolbarItems.Add(_deselectAllItem);
        }
    }

    protected override void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        UpdateToolbarItems();
    }

    protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
    {
        base.OnNavigatingFrom(args);
        ViewModel.ToolbarVisibilityChanged -= OnToolbarVisibilityChanged;
    }
}
