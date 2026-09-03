using Shelfly.App.Enums;
using Shelfly.App.Features.Library.Services;

namespace Shelfly.App.ViewModels;

public class SortOptionDisplay(SortCriterion criterion, string displayName)
{
    public SortCriterion Criterion { get; } = criterion;
    public string DisplayName { get; } = displayName;

    public override string ToString() => DisplayName;
}
