namespace Shelfly.App.Controls;

public sealed class MultiSelectSelectionChangedEventArgs(
    IReadOnlyList<object> previousSelection,
    IReadOnlyList<object> currentSelection)
    : EventArgs
{
    public IReadOnlyList<object> PreviousSelection { get; } = previousSelection;

    public IReadOnlyList<object> CurrentSelection { get; } = currentSelection;
}