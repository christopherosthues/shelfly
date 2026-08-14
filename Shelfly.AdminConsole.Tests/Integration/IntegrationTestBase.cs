using Shelfly.AdminConsole.Tests.Helpers;

namespace Shelfly.AdminConsole.Tests.Integration;

public class IntegrationTestBase
{
    [ClassDataSource<MongoDbTestContainer>(Shared = SharedType.PerClass)]
    public required MongoDbTestContainer MongoDbContainer { get; init; }
}