using Shelfly.App.Controls;
using Shelfly.App.Pages;
using Shelfly.App.Resources.Localization;

namespace Shelfly.App.Features.Trash;

public partial class TrashListPage : ShelflyContentPageBase
{
    private ToolbarItem? _restoreAllItem;
    private ToolbarItem? _deleteAllItem;
    private ToolbarItem? _restoreSelectedItem;
    private ToolbarItem? _deleteSelectedItem;
    private ToolbarItem? _selectAllItem;
    private ToolbarItem? _deselectAllItem;

    private TrashListViewModel ViewModel => (TrashListViewModel) BindingContext;

    public TrashListPage(TrashListViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();

        CreateToolbarItems(viewModel);
        viewModel.ToolbarVisibilityChanged += OnToolbarVisibilityChanged;
        MultiSelectView.SelectionChanged += OnSelectionChanged;
    }

    private void CreateToolbarItems(TrashListViewModel viewModel)
    {
        _selectAllItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageSelectAllButtonText,
            IconImageSource = "select_all_icon.svg",
            Command = viewModel.SelectAllCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_selectAllItem, AppResources.TrashListPageSelectAllDescription);

        _restoreAllItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageRestoreAllButtonText,
            IconImageSource = "restore_all_icon.svg",
            Command = viewModel.RestoreAllCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_restoreAllItem, AppResources.TrashListPageRestoreAllDescription);

        _deleteAllItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageDeleteAllButtonText,
            IconImageSource = "delete_all_icon.svg",
            Command = viewModel.DeleteAllCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_deleteAllItem, AppResources.TrashListPageDeleteAllDescription);

        _restoreSelectedItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageRestoreSelectedButtonText,
            IconImageSource = "restore_icon.svg",
            Command = viewModel.RestoreSelectedCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_restoreSelectedItem, AppResources.TrashListPageRestoreSelectedDescription);

        _deleteSelectedItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageDeleteSelectedButtonText,
            IconImageSource = "delete_icon.svg",
            Command = viewModel.DeleteSelectedCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_deleteSelectedItem, AppResources.TrashListPageDeleteSelectedDescription);

        _deselectAllItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageDeselectAllButtonText,
            IconImageSource = "deselect_all_icon.svg",
            Command = viewModel.DeselectAllCommand,
            Order = ToolbarItemOrder.Secondary
        };
        SemanticProperties.SetDescription(_selectAllItem, AppResources.TrashListPageDeselectAllDescription);
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
        TrashListViewModel viewModel = ViewModel;
        ToolbarItems.Clear();

        if (viewModel.IsSelectAllVisible && _selectAllItem is not null)
        {
            ToolbarItems.Add(_selectAllItem);
        }

        if (viewModel.IsRestoreAllVisible && _restoreAllItem is not null)
        {
            ToolbarItems.Add(_restoreAllItem);
        }

        if (viewModel.IsDeleteAllVisible && _deleteAllItem is not null)
        {
            ToolbarItems.Add(_deleteAllItem);
        }

        if (viewModel.IsRestoreSelectedVisible && _restoreSelectedItem is not null)
        {
            ToolbarItems.Add(_restoreSelectedItem);
        }

        if (viewModel.IsDeleteSelectedVisible && _deleteSelectedItem is not null)
        {
            ToolbarItems.Add(_deleteSelectedItem);
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
