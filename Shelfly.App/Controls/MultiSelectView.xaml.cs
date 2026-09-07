using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Shelfly.App.Resources.Localization;

namespace Shelfly.App.Controls;

// ============================================================================
// MultiSelectView
// ============================================================================

public partial class MultiSelectView : ContentView
{
    private readonly CollectionView _collectionView;

    // Selection is stored against the domain objects.
    //
    // We cannot store selection only on the visual element because
    // CollectionView recycles visual elements.
    private readonly HashSet<object> _selectedItems = [];

    private INotifyCollectionChanged? _observableItems;
    private INotifyCollectionChanged? _observableSelectedItems;

    private bool _updatingSelectionMode;
    private bool _updatingSelectedItems;

    public MultiSelectView()
    {
        _collectionView = new CollectionView
        {
            // IMPORTANT:
            //
            // We deliberately don't use CollectionView's selection system.
            //
            // Selection is completely controlled by this component.
            SelectionMode = SelectionMode.None,

            ItemsLayout = new LinearItemsLayout(ItemsLayoutOrientation.Vertical)
            {
                ItemSpacing = 0
            }
        };

        _collectionView.SetBinding(ItemsView.ItemsSourceProperty, new Binding(nameof(ItemsSource), source: this));

        _collectionView.ItemTemplate =
            new DataTemplate(() =>
            {
                if (ItemTemplate == null)
                {
                    return new Label
                    {
                        Text = "MultiSelectView: ItemTemplate is required.",
                        Padding = 20
                    };
                }

                return new MultiSelectItemContainer(this, ItemTemplate);
            });

        Content = _collectionView;
    }

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(MultiSelectView),
            null,
            propertyChanged: OnItemsSourceChanged);

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        MultiSelectView control = (MultiSelectView)bindable;

        control.UnsubscribeFromItems();

        if (newValue is INotifyCollectionChanged collection)
        {
            control._observableItems = collection;

            collection.CollectionChanged += control.OnItemsCollectionChanged;
        }

        control.RemoveSelectionsThatNoLongerExist();
    }

    private void UnsubscribeFromItems()
    {
        if (_observableItems == null)
        {
            return;
        }

        _observableItems.CollectionChanged -= OnItemsCollectionChanged;

        _observableItems = null;
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        MainThread.BeginInvokeOnMainThread(RemoveSelectionsThatNoLongerExist);
    }

    public static readonly BindableProperty EmptyViewProperty =
        BindableProperty.Create(
            nameof(EmptyView),
            typeof(View),
            typeof(ItemsView),
            null,
            propertyChanged: OnEmptyViewChanged);

    public View? EmptyView
    {
        get => (View?)GetValue(EmptyViewProperty);
        set => SetValue(EmptyViewProperty, value);
    }

    private static void OnEmptyViewChanged(
        BindableObject bindable,
        object? oldValue,
        object? newValue)
    {
        MultiSelectView control = (MultiSelectView)bindable;

        control._collectionView.EmptyView = control.EmptyView;
    }

    public static readonly BindableProperty EmptyTemplateProperty =
        BindableProperty.Create(
            nameof(EmptyTemplate),
            typeof(DataTemplate),
            typeof(MultiSelectView),
            null,
            propertyChanged: OnEmptyTemplateChanged);

    public DataTemplate? EmptyTemplate
    {
        get => (DataTemplate?)GetValue(EmptyTemplateProperty);
        set => SetValue(EmptyTemplateProperty, value);
    }

    private static void OnEmptyTemplateChanged(
        BindableObject bindable,
        object? oldValue,
        object? newValue)
    {
        MultiSelectView control = (MultiSelectView)bindable;

        control._collectionView.EmptyView =
            new DataTemplate(() =>
            {
                if (control.EmptyTemplate == null)
                {
                    return new Label
                    {
                        Text = AppResources.MultiSelectViewEmptyText,
                        Padding = 20
                    };
                }

                return control.EmptyTemplate;
            });
    }

    public static readonly BindableProperty ItemTemplateProperty =
        BindableProperty.Create(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(MultiSelectView),
            null,
            propertyChanged: OnItemTemplateChanged);

    public DataTemplate? ItemTemplate
    {
        get => (DataTemplate?)GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    private static void OnItemTemplateChanged(
        BindableObject bindable,
        object? oldValue,
        object? newValue)
    {
        MultiSelectView control = (MultiSelectView)bindable;

        control._collectionView.ItemTemplate =
            new DataTemplate(() =>
            {
                if (control.ItemTemplate == null)
                {
                    return new Label
                    {
                        Text = "MultiSelectView: ItemTemplate is required.",
                        Padding = 20
                    };
                }

                return new MultiSelectItemContainer(control, control.ItemTemplate);
            });
    }

    public static readonly BindableProperty ItemTapCommandProperty =
        BindableProperty.Create(
            nameof(ItemTapCommand),
            typeof(ICommand),
            typeof(MultiSelectView));

    public ICommand? ItemTapCommand
    {
        get => (ICommand?)GetValue(ItemTapCommandProperty);
        set => SetValue(ItemTapCommandProperty, value);
    }

    public static readonly BindableProperty SelectedItemsProperty =
        BindableProperty.Create(
            nameof(SelectedItems),
            typeof(IList),
            typeof(MultiSelectView),
            null,
            BindingMode.TwoWay,
            propertyChanged: OnSelectedItemsChanged);

    public IList? SelectedItems
    {
        get => (IList?)GetValue(SelectedItemsProperty);
        set => SetValue(SelectedItemsProperty, value);
    }

    private static void OnSelectedItemsChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        MultiSelectView control = (MultiSelectView)bindable;

        control.UnsubscribeFromSelectedItems();

        if (newValue is INotifyCollectionChanged collection)
        {
            control._observableSelectedItems = collection;
            collection.CollectionChanged += control.OnSelectedItemsCollectionChanged;
        }

        if (control._updatingSelectedItems)
        {
            return;
        }

        control.ApplyExternalSelection(newValue as IList);
    }

    public static readonly BindableProperty IsSelectionModeProperty =
        BindableProperty.Create(
            nameof(IsSelectionMode),
            typeof(bool),
            typeof(MultiSelectView),
            false,
            BindingMode.TwoWay,
            propertyChanged: OnIsSelectionModeChanged);

    public bool IsSelectionMode
    {
        get => (bool)GetValue(IsSelectionModeProperty);
        set => SetValue(IsSelectionModeProperty, value);
    }

    private static void OnIsSelectionModeChanged(BindableObject bindable, object? oldValue, object? newValue)
    {
        MultiSelectView control = (MultiSelectView)bindable;

        if (control._updatingSelectionMode)
        {
            return;
        }

        if (newValue is false)
        {
            control.ClearSelection();
        }
    }

    public int SelectedCount => _selectedItems.Count;

    public static readonly BindableProperty LongPressDurationProperty =
        BindableProperty.Create(
            nameof(LongPressDuration),
            typeof(int),
            typeof(MultiSelectView),
            500,
            validateValue: (_, value) => value is int and >= 100);


    public int LongPressDuration
    {
        get => (int)GetValue(LongPressDurationProperty);
        set => SetValue(LongPressDurationProperty, value);
    }

    public event EventHandler<MultiSelectSelectionChangedEventArgs>? SelectionChanged;

    internal void HandleTap(object item, IMultiSelectItemView itemView)
    {
        if (IsSelectionMode)
        {
            ToggleSelection(item, itemView);

            return;
        }

        if (ItemTapCommand?.CanExecute(item) == true)
        {
            ItemTapCommand.Execute(item);
        }
    }


    internal void HandleLongPress(object item, IMultiSelectItemView itemView)
    {
        if (!IsSelectionMode)
        {
            SetSelectionMode(true);
        }

        Select(item, itemView);
    }

    public void Select(object item, IMultiSelectItemView? itemView = null)
    {
        List<object> previousSelection  = [.. _selectedItems];

        if (!_selectedItems.Add(item))
        {
            return;
        }

        if (itemView != null)
        {
            itemView.IsSelected = true;
        }

        AddToSelectedItems(item);

        SetSelectionMode(true);

        RaiseSelectionChanged(previousSelection);
    }


    public void Deselect(object item, IMultiSelectItemView? itemView = null)
    {
        List<object> previousSelection  = [.. _selectedItems];

        if (!_selectedItems.Remove(item))
        {
            return;
        }

        itemView?.IsSelected = false;

        RemoveFromSelectedItems(item);

        if (_selectedItems.Count == 0)
        {
            SetSelectionMode(false);
        }

        RaiseSelectionChanged(previousSelection);
    }


    public void ToggleSelection(object item, IMultiSelectItemView itemView)
    {
        if (_selectedItems.Contains(item))
        {
            Deselect(item, itemView);
        }
        else
        {
            Select(item, itemView);
        }
    }

    public void ClearSelection()
    {
        if (_selectedItems.Count == 0)
        {
            SetSelectionMode(false);
            return;
        }

        List<object> previousSelection  = [.. _selectedItems];

        _selectedItems.Clear();

        foreach (object item in previousSelection)
        {
            RemoveFromSelectedItems(item);

            SetRealizedItemSelectedState(item, false);
        }

        SetSelectionMode(false);

        RaiseSelectionChanged(previousSelection);
    }

    public void SelectAll()
    {
        if (ItemsSource == null)
        {
            return;
        }

        List<object> previousSelection = [.. _selectedItems];

        SetSelectionMode(true);

        foreach (object? item in ItemsSource)
        {
            if (item == null)
            {
                continue;
            }

            if (!_selectedItems.Add(item))
            {
                continue;
            }

            AddToSelectedItems(item);

            SetRealizedItemSelectedState(item, true);
        }

        RaiseSelectionChanged(previousSelection);
    }

    private void ApplyExternalSelection(IList? externalSelection)
    {
        HashSet<object> desired =
            externalSelection?
                .Cast<object>()
                .ToHashSet()
            ?? [];

        foreach (object item in _selectedItems
                     .Where(x => !desired.Contains(x))
                     .ToList())
        {
            _selectedItems.Remove(item);

            SetRealizedItemSelectedState(item, false);
        }

        foreach (object item in desired)
        {
            _selectedItems.Add(item);

            SetRealizedItemSelectedState(item, true);
        }

        SetSelectionMode(_selectedItems.Count > 0);

        OnPropertyChanged(nameof(SelectedCount));
    }

    private void AddToSelectedItems(object item)
    {
        if (SelectedItems == null)
        {
            return;
        }

        if (SelectedItems.Contains(item))
        {
            return;
        }

        _updatingSelectedItems = true;

        try
        {
            SelectedItems.Add(item);
        }
        finally
        {
            _updatingSelectedItems = false;
        }
    }

    private void RemoveFromSelectedItems(object item)
    {
        if (SelectedItems == null)
        {
            return;
        }

        if (!SelectedItems.Contains(item))
        {
            return;
        }

        _updatingSelectedItems = true;

        try
        {
            SelectedItems.Remove(item);
        }
        finally
        {
            _updatingSelectedItems = false;
        }
    }

    private void RemoveSelectionsThatNoLongerExist()
    {
        HashSet<object> currentItems = ItemsSource?.Cast<object>().ToHashSet() ?? [];

        List<object> previousSelection  = [.. _selectedItems];

        List<object> invalidItems =
        [
            .. _selectedItems
                .Where(item => !currentItems.Contains(item))
        ];

        if (invalidItems.Count == 0)
        {
            return;
        }

        foreach (var item in invalidItems)
        {
            _selectedItems.Remove(item);

            RemoveFromSelectedItems(item);
        }

        SetSelectionMode(_selectedItems.Count > 0);

        RaiseSelectionChanged(previousSelection);
    }

    private void SetSelectionMode(bool value)
    {
        if (IsSelectionMode == value)
        {
            return;
        }

        _updatingSelectionMode = true;

        try
        {
            IsSelectionMode = value;
        }
        finally
        {
            _updatingSelectionMode = false;
        }
    }

    internal bool IsItemSelected(object item)
    {
        return _selectedItems.Contains(item);
    }

    private void SetRealizedItemSelectedState(object item, bool isSelected)
    {
        foreach (MultiSelectItemContainer container in FindRealizedContainers(_collectionView))
        {
            if (ReferenceEquals(container.BindingContext, item))
            {
                container.ItemView.IsSelected = isSelected;
                break;
            }
        }
    }

    private static IEnumerable<MultiSelectItemContainer> FindRealizedContainers(Element parent)
    {
        return parent.GetVisualTreeDescendants().OfType<MultiSelectItemContainer>();
    }

    private void RaiseSelectionChanged(IReadOnlyList<object> previousSelection)
    {
        OnPropertyChanged(nameof(SelectedCount));

        SelectionChanged?.Invoke(this, new MultiSelectSelectionChangedEventArgs(previousSelection, [.. _selectedItems]));
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler == null)
        {
            UnsubscribeFromItems();
        }
    }

    private void UnsubscribeFromSelectedItems()
    {
        if (_observableSelectedItems == null)
        {
            return;
        }

        _observableSelectedItems.CollectionChanged -= OnSelectedItemsCollectionChanged;
        _observableSelectedItems = null;
    }

    private void OnSelectedItemsCollectionChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e)
    {
        if (_updatingSelectedItems)
        {
            return;
        }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            ApplyExternalSelection(SelectedItems);
        });
    }
}