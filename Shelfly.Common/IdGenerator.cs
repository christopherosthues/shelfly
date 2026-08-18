namespace Shelfly.Common;

public static class IdGenerator
{
    public static Guid NewId() => Guid.CreateVersion7();
}
