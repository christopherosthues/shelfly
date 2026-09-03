using Shelfly.App.Features.Trash.ViewModels;
using Shelfly.App.Pages;
using Shelfly.App.Resources.Localization;

namespace Shelfly.App.Features.Trash.Pages;

public partial class TrashListPage : ShelflyContentPageBase
{
    private ToolbarItem? _restoreAllItem;
    private ToolbarItem? _deleteAllItem;
    private ToolbarItem? _restoreSelectedItem;
    private ToolbarItem? _deleteSelectedItem;
    private ToolbarItem? _doneItem;

    private TrashListViewModel ViewModel => (TrashListViewModel) BindingContext;

    public TrashListPage(TrashListViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();

        CreateToolbarItems(viewModel);
        viewModel.ToolbarVisibilityChanged += OnToolbarVisibilityChanged;
    }

    private void CreateToolbarItems(TrashListViewModel viewModel)
    {
        _restoreAllItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageRestoreAllButtonText,
            IconImageSource = "restore_icon.svg",
            Command = viewModel.RestoreAllCommand
        };
        SemanticProperties.SetDescription(_restoreAllItem, AppResources.TrashListPageRestoreAllDescription);

        _deleteAllItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageDeleteAllButtonText,
            IconImageSource = "delete_icon.svg",
            Command = viewModel.DeleteAllCommand
        };
        SemanticProperties.SetDescription(_deleteAllItem, AppResources.TrashListPageDeleteAllDescription);

        _restoreSelectedItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageRestoreSelectedButtonText,
            IconImageSource = "restore_icon.svg",
            Command = viewModel.RestoreSelectedCommand
        };
        SemanticProperties.SetDescription(_restoreSelectedItem, AppResources.TrashListPageRestoreSelectedDescription);

        _deleteSelectedItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageDeleteSelectedButtonText,
            IconImageSource = "delete_icon.svg",
            Command = viewModel.DeleteSelectedCommand
        };
        SemanticProperties.SetDescription(_deleteSelectedItem, AppResources.TrashListPageDeleteSelectedDescription);

        _doneItem = new ToolbarItem
        {
            Text = AppResources.TrashListPageDoneButtonText,
            IconImageSource = "check_icon.svg",
            Command = viewModel.ExitSelectionModeCommand
        };
        SemanticProperties.SetDescription(_doneItem, AppResources.TrashListPageDoneSelectionDescription);
    }

    private void OnToolbarVisibilityChanged(object? sender, EventArgs e)
    {
        UpdateToolbarItems();
    }

    private void UpdateToolbarItems()
    {
        TrashListViewModel viewModel = ViewModel;
        ToolbarItems.Clear();

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

        if (viewModel.IsSelectionMode && _doneItem is not null)
        {
            ToolbarItems.Add(_doneItem);
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
