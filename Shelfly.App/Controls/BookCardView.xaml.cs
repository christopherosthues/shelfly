using System.Windows.Input;

namespace Shelfly.App.Controls;

public partial class BookCardView : ContentView
{
    public static readonly BindableProperty IsSelectedProperty =
        BindableProperty.Create(nameof(IsSelected), typeof(bool), typeof(BookCardView), false,
            propertyChanged: OnIsSelectedPropertyChanged);

    public static readonly BindableProperty LongPressCommandProperty =
        BindableProperty.Create(nameof(LongPressCommand), typeof(ICommand), typeof(BookCardView), null);

    public static readonly BindableProperty TapCommandProperty =
        BindableProperty.Create(nameof(TapCommand), typeof(ICommand), typeof(BookCardView), null);

    public static readonly BindableProperty TapCommandParameterProperty =
        BindableProperty.Create(nameof(TapCommandParameter), typeof(object), typeof(BookCardView), null);

    public bool IsSelected
    {
        get => (bool)GetValue(IsSelectedProperty);
        set => SetValue(IsSelectedProperty, value);
    }

    public ICommand? LongPressCommand
    {
        get => (ICommand?)GetValue(LongPressCommandProperty);
        set => SetValue(LongPressCommandProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public object? TapCommandParameter
    {
        get => (object?)GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
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

    private DateTime? _pressTime;
    private const int LongPressThreshold = 500; // milliseconds

    public BookCardView()
    {
        InitializeComponent();

        PointerGestureRecognizer pointerGesture = new();
        pointerGesture.PointerPressed += OnPointerPressed;
        pointerGesture.PointerReleased += OnPointerReleased;
        GestureRecognizers.Add(pointerGesture);
    }

    private void OnPointerPressed(object? sender, PointerEventArgs e)
    {
        _pressTime = DateTime.Now;
    }

    private void OnPointerReleased(object? sender, PointerEventArgs e)
    {
        if (_pressTime is not null)
        {
            int elapsed = (int)(DateTime.Now - _pressTime.Value).TotalMilliseconds;
            _pressTime = null;

            object? parameter = BindingContext;
            if (elapsed >= LongPressThreshold)
            {
                OnLongPressed(parameter);
            }
            else
            {
                OnShortPressed();
            }
        }
    }

    private void OnLongPressed(object parameter)
    {
        IsSelected = !IsSelected;

        HapticFeedback.Default.Perform(HapticFeedbackType.LongPress);

        if (LongPressCommand?.CanExecute(parameter) ?? false)
        {
            LongPressCommand.Execute(parameter);
        }
    }

    private void OnShortPressed()
    {
        if (IsSelected)
        {
            IsSelected = false;
        }
        else if (TapCommand?.CanExecute(TapCommandParameter) ?? true)
        {
            TapCommand?.Execute(TapCommandParameter);
        }
    }
}
