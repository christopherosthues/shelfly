namespace Shelfly.App.Messages;

public sealed class LibraryChangedMessage
{
    public static readonly LibraryChangedMessage Instance = new();

    private LibraryChangedMessage() { }
}
