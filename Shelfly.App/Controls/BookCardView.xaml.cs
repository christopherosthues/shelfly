namespace Shelfly.App.Controls;

public partial class BookCardView : ContentView, IMultiSelectItemView
{
    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(BookCardView), false,
            propertyChanged: OnIsSelectedPropertyChanged);

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    private static void OnIsSelectedPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BookCardView view)
        {
            bool isSelected = (bool)newValue;
            Grid? indicator = view.Selection;

            if (indicator is not null)
            {
                double targetRotation = isSelected ? 0 : 90;
                _ = indicator.RotateYToAsync(targetRotation, 300, Easing.SinInOut);
            }
        }
    }

    public static readonly BindableProperty LeftItemProperty =
        BindableProperty.Create(nameof(LeftItem), typeof(SwipeItem), typeof(BookCardView), null,
            propertyChanged: OnSwipeItemChanged);

    public SwipeItem? LeftItem
    {
        get => (SwipeItem?)GetValue(LeftItemProperty);
        set => SetValue(LeftItemProperty, value);
    }

    public static readonly BindableProperty RightItemProperty =
        BindableProperty.Create(nameof(RightItem), typeof(SwipeItem), typeof(BookCardView), null,
            propertyChanged: OnSwipeItemChanged);

    public SwipeItem? RightItem
    {
        get => (SwipeItem?)GetValue(RightItemProperty);
        set => SetValue(RightItemProperty, value);
    }

    private static void OnSwipeItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is BookCardView view)
        {
            view.SwipeView.LeftItems = view.LeftItem is { } leftItem ? [leftItem] : [];
            view.SwipeView.RightItems = view.RightItem is { } rightItem ? [rightItem] : [];
        }
    }

    public BookCardView()
    {
        InitializeComponent();
    }
}
