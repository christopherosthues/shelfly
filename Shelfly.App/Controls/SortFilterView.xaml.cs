using System.Collections.Generic;
using System.Windows.Input;
using Microsoft.Maui.Controls;

namespace Shelfly.App.Controls;

public partial class SortFilterView : FlexLayout
{
    public static readonly BindableProperty SearchTextProperty =
        BindableProperty.Create(nameof(SearchText), typeof(string), typeof(SortFilterView), string.Empty,
            BindingMode.TwoWay);

    public static readonly BindableProperty SearchPlaceholderProperty =
        BindableProperty.Create(nameof(SearchPlaceholder), typeof(string), typeof(SortFilterView), null);

    public static readonly BindableProperty SearchDescriptionProperty =
        BindableProperty.Create(nameof(SearchDescription), typeof(string), typeof(SortFilterView), null);

    public static readonly BindableProperty SearchCommandProperty =
        BindableProperty.Create(nameof(SearchCommand), typeof(ICommand), typeof(SortFilterView), null);

    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable<object?>), typeof(SortFilterView), null);

    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(SortFilterView), 0,
            BindingMode.TwoWay);

    public static readonly BindableProperty SortIconSourceProperty =
        BindableProperty.Create(nameof(SortIconSource), typeof(string), typeof(SortFilterView), null);

    public static readonly BindableProperty SortDirectionDescriptionProperty =
        BindableProperty.Create(nameof(SortDirectionDescription), typeof(string), typeof(SortFilterView), null);

    public static readonly BindableProperty SortDescriptionProperty =
        BindableProperty.Create(nameof(SortDescription), typeof(string), typeof(SortFilterView), null);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(SortFilterView), null);

    public static readonly BindableProperty ToggleCommandProperty =
        BindableProperty.Create(nameof(ToggleCommand), typeof(ICommand), typeof(SortFilterView), null);

    public static readonly BindableProperty SortCommandProperty =
        BindableProperty.Create(nameof(SortCommand), typeof(ICommand), typeof(SortFilterView), null);

    public string SearchText
    {
        get => (string)GetValue(SearchTextProperty);
        set => SetValue(SearchTextProperty, value);
    }

    public string SearchPlaceholder
    {
        get => (string)GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    public string SearchDescription
    {
        get => (string)GetValue(SearchDescriptionProperty);
        set => SetValue(SearchDescriptionProperty, value);
    }

    public ICommand? SearchCommand
    {
        get => (ICommand?)GetValue(SearchCommandProperty);
        set => SetValue(SearchCommandProperty, value);
    }

    public IEnumerable<object?> ItemsSource
    {
        get => (IEnumerable<object?>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public string SortIconSource
    {
        get => (string)GetValue(SortIconSourceProperty);
        set => SetValue(SortIconSourceProperty, value);
    }

    public string SortDirectionDescription
    {
        get => (string)GetValue(SortDirectionDescriptionProperty);
        set => SetValue(SortDirectionDescriptionProperty, value);
    }

    public string SortDescription
    {
        get => (string)GetValue(SortDescriptionProperty);
        set => SetValue(SortDescriptionProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public ICommand? ToggleCommand
    {
        get => (ICommand?)GetValue(ToggleCommandProperty);
        set => SetValue(ToggleCommandProperty, value);
    }

    public ICommand? SortCommand
    {
        get => (ICommand?)GetValue(SortCommandProperty);
        set => SetValue(SortCommandProperty, value);
    }

    public SortFilterView()
    {
        InitializeComponent();
    }
}
