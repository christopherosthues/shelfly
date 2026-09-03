using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Shelfly.App.Features.Library.Services;
using Shelfly.App.Resources.Localization;

namespace Shelfly.App.ViewModels;

public abstract partial class SortableListViewModelBase : ShelflyViewModelBase
{
    [ObservableProperty]
    public partial string SearchQuery { get; set; } = string.Empty;

    [ObservableProperty]
    public partial int SelectedSortOptionIndex { get; set; } = 0;

    [ObservableProperty]
    public partial SortCriterion SortCriterion { get; set; } = SortCriterion.Title;

    [ObservableProperty]
    public partial SortDirection SortDirection { get; set; } = SortDirection.Ascending;

    [ObservableProperty]
    public partial bool IsLoading { get; set; } = false;

    public List<SortOptionDisplay> SortOptions { get; } =
    [
        new SortOptionDisplay(SortCriterion.Title, AppResources.BookListPageSortByTitle),
        new SortOptionDisplay(SortCriterion.Author, AppResources.BookListPageSortByAuthor),
        new SortOptionDisplay(SortCriterion.Publisher, AppResources.BookListPageSortByPublisher),
        new SortOptionDisplay(SortCriterion.PublishDate, AppResources.BookListPageSortByPublishDate)
    ];

    public string SortIconSource => SortDirection == SortDirection.Ascending ? "sort_asc.svg" : "sort_desc.svg";

    public string SortDirectionDescription => SortDirection == SortDirection.Ascending
        ? AppResources.SortDirectionAscending
        : AppResources.SortDirectionDescending;

    protected abstract Task LoadSortedItemsAsync(string query, SortCriterion criterion, SortDirection direction, CancellationToken cancellationToken);

    partial void OnSortCriterionChanged(SortCriterion value)
    {
        SelectedSortOptionIndex = SortOptions.FindIndex(o => o.Criterion == value);
        OnSortCriterionChangedCore(value);
    }

    partial void OnSearchQueryChanged(string value)
    {
        SearchCommand.Execute(null);
        OnSearchQueryChangedCore(value);
    }

    protected virtual void OnSortCriterionChangedCore(SortCriterion value) { }

    protected virtual void OnSearchQueryChangedCore(string value) { }

    [RelayCommand]
    private async Task SearchAsync(CancellationToken cancellationToken)
    {
        await LoadSortedItemsAsync(SearchQuery, SortCriterion, SortDirection, cancellationToken);
    }

    [RelayCommand]
    private async Task SortAsync(SortOptionDisplay? option)
    {
        if (option is not null)
        {
            SortCriterion = option.Criterion;
            SelectedSortOptionIndex = SortOptions.FindIndex(o => o.Criterion == option.Criterion);
        }

        await LoadSortedItemsAsync(SearchQuery, SortCriterion, SortDirection, CancellationToken.None);
    }

    [RelayCommand]
    private async Task ToggleSortDirectionAsync()
    {
        SortDirection = SortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
        OnPropertyChanged(nameof(SortIconSource));
        OnPropertyChanged(nameof(SortDirectionDescription));
        await LoadSortedItemsAsync(SearchQuery, SortCriterion, SortDirection, CancellationToken.None);
    }

    protected async Task ExecuteWithLoadingAsync(Func<Task> action)
    {
        IsLoading = true;
        try
        {
            await action();
        }
        finally
        {
            IsLoading = false;
        }
    }
}
