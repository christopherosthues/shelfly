using Shelfly.Common;

namespace Shelfly.App.Controls;

public partial class ValidatingEntry : ContentView
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(ValidatingEntry), string.Empty,
            propertyChanged: OnTextPropertyChanged);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidatingEntry view)
        {
            if (view.Validator is not null)
            {
                Result<bool> result = view.Validator.Validate(newValue, view.ValidatorParameter);
                if (result.IsSuccess)
                {
                    view.IsValid = true;
                    view.ErrorText = null;
                }
                else
                {
                    view.IsValid = false;
                    view.ErrorText = result.Error;
                }
            }
        }
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(ValidatingEntry), null);

    public string? Placeholder
    {
        get => (string?)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public static readonly BindableProperty MaxLengthProperty =
        BindableProperty.Create(nameof(MaxLength), typeof(int), typeof(ValidatingEntry), int.MaxValue);

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public static readonly BindableProperty IsValidProperty =
        BindableProperty.Create(nameof(IsValid), typeof(bool), typeof(ValidatingEntry), true,
            propertyChanged: OnIsValidPropertyChanged);

    public bool IsValid
    {
        get => (bool)GetValue(IsValidProperty);
        set => SetValue(IsValidProperty, value);
    }

    private static void OnIsValidPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidatingEntry view)
        {
            bool isValid = (bool)newValue;
            view.ErrorText = isValid ? null : view.ErrorText;
        }
    }

    public static readonly BindableProperty ErrorTextProperty =
        BindableProperty.Create(nameof(ErrorText), typeof(string), typeof(ValidatingEntry), null);

    public string? ErrorText
    {
        get => (string?)GetValue(ErrorTextProperty);
        set => SetValue(ErrorTextProperty, value);
    }

    public static readonly BindableProperty ValidatorProperty =
        BindableProperty.Create(nameof(Validator), typeof(IValidator), typeof(ValidatingEntry), null);

    public IValidator? Validator
    {
        get => (IValidator?)GetValue(ValidatorProperty);
        set => SetValue(ValidatorProperty, value);
    }

    public static readonly BindableProperty ValidatorParameterProperty =
        BindableProperty.Create(nameof(ValidatorParameter), typeof(object), typeof(ValidatingEntry), null);

    public object? ValidatorParameter
    {
        get => (object?)GetValue(ValidatorParameterProperty);
        set => SetValue(ValidatorParameterProperty, value);
    }


    public ValidatingEntry()
    {
        InitializeComponent();
    }
}
