using CommunityToolkit.Maui.Behaviors;

namespace Shelfly.App.Controls;

// ============================================================================
// MultiSelectItemContainer
// ============================================================================
//
// Internal wrapper around the user's DataTemplate.
//
// The CollectionView creates:
//
//     MultiSelectItemContainer
//             |
//             +-- user's IMultiSelectItemView
//
// BindingContext remains the DOMAIN OBJECT.
//
// ============================================================================
public partial class MultiSelectItemContainer : ContentView
{
    private readonly MultiSelectView _owner;
    private readonly DataTemplate _itemTemplate;

    private IMultiSelectItemView? _itemView;
    private object? _item;

    private TouchBehavior? _touchBehavior;

    public MultiSelectItemContainer(MultiSelectView owner, DataTemplate itemTemplate)
    {
        _owner = owner;
        _itemTemplate = itemTemplate;

        BindingContextChanged += OnBindingContextChanged;

        CreateItemView();
    }

    public IMultiSelectItemView ItemView =>
        _itemView ?? throw new InvalidOperationException("The item view has not been created.");

    private void CreateItemView()
    {
        object? content = _itemTemplate.CreateContent();

        if (content is not IMultiSelectItemView itemView)
        {
            throw new InvalidOperationException(
                $"The MultiSelectView ItemTemplate must create a View " +
                $"implementing {nameof(IMultiSelectItemView)}. " +
                $"The template created " +
                $"{content?.GetType().FullName ?? "<null>"}.");
        }

        _itemView = itemView;
        Content = (View)itemView;
        InstallGestures();
    }

    private void OnBindingContextChanged(
        object? sender,
        EventArgs e)
    {
        _item = BindingContext;

        if (_item == null || _itemView == null)
        {
            return;
        }

        _itemView.IsSelected = _owner.IsItemSelected(_item);
    }

    private void InstallGestures()
    {
        if (_itemView is not View view)
        {
            return;
        }

        _touchBehavior = new TouchBehavior
        {
            LongPressDuration = _owner.LongPressDuration,
            Command = new Command(OnTapped),
            LongPressCommand = new Command(OnLongPressed)
        };

        if (view is ContentView contentView)
        {
            if (contentView.Content is SwipeView swipeView)
            {
                swipeView.Content.Behaviors.Add(_touchBehavior);
            }
            else
            {
                contentView.Content.Behaviors.Add(_touchBehavior);
            }
        }
        else
        {
            view.Behaviors.Add(_touchBehavior);
        }
    }

    private void OnTapped()
    {
        if (_item == null || _itemView == null)
        {
            return;
        }

        _owner.HandleTap(_item, _itemView);
    }

    private void OnLongPressed()
    {
        if (_item == null || _itemView == null)
        {
            return;
        }

        _owner.HandleLongPress(_item, _itemView);
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();

        if (Handler != null)
        {
            return;
        }

        BindingContextChanged -= OnBindingContextChanged;

        if (_touchBehavior != null && _itemView is View view)
        {
            view.Behaviors.Remove(_touchBehavior);
        }
    }
}