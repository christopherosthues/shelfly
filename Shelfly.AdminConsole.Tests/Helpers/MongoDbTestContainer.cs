using Testcontainers.MongoDb;
using TUnit.Core.Interfaces;

namespace Shelfly.AdminConsole.Tests.Helpers;

public class MongoDbTestContainer : IAsyncInitializer, IAsyncDisposable
{
    private MongoDbContainer? _mongoDbContainer;

    public async Task InitializeAsync()
    {
        _mongoDbContainer = new MongoDbBuilder("mongodb/mongodb-community-server:8.3.8-ubuntu2204-slim").Build();
        await _mongoDbContainer.StartAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_mongoDbContainer != null)
        {
            await _mongoDbContainer.StopAsync();
            await _mongoDbContainer.DisposeAsync();
        }
    }
}