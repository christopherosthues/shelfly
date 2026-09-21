using MongoDB.Bson;
using MongoDB.Driver;

namespace Shelfly.Api.Features.Auth.Services;

public class RateLimitService(IMongoDatabase mongoDatabase, ILogger<RateLimitService> logger)
{
    private readonly IMongoCollection<LoginAttemptRecord> _loginAttempts = mongoDatabase.GetCollection<LoginAttemptRecord>("login_attempts");
    private readonly ILogger<RateLimitService> _logger = logger;

    public async Task<bool> IsLockedOutAsync(string email, CancellationToken cancellationToken)
    {
        var windowStart = DateTimeOffset.UtcNow.AddMinutes(-15);

        var filter = Builders<LoginAttemptRecord>.Filter.And(
            Builders<LoginAttemptRecord>.Filter.Eq(r => r.Email, email),
            Builders<LoginAttemptRecord>.Filter.Gte(r => r.Timestamp, windowStart));

        var count = await _loginAttempts.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        bool lockedOut = count >= 10;

        if (lockedOut)
        {
            _logger.LogWarning("Account {Email} temporarily locked after {Count} failures in 15 minutes", email, count);
        }

        return lockedOut;
    }

    public async Task<RateLimitStatus> RecordLoginAttemptAsync(string email, bool success, CancellationToken cancellationToken)
    {
        var record = new LoginAttemptRecord(Guid.CreateVersion7(), email, DateTimeOffset.UtcNow, success);

        await _loginAttempts.InsertOneAsync(record, cancellationToken: cancellationToken);

        if (!success)
        {
            var windowStart = DateTimeOffset.UtcNow.AddMinutes(-1);

            var filter = Builders<LoginAttemptRecord>.Filter.And(
                Builders<LoginAttemptRecord>.Filter.Eq(r => r.Email, email),
                Builders<LoginAttemptRecord>.Filter.Gte(r => r.Timestamp, windowStart),
                Builders<LoginAttemptRecord>.Filter.Eq(r => r.Success, false));

            var recentFailures = await _loginAttempts.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            if (recentFailures >= 5)
            {
                return RateLimitStatus.RateLimited;
            }
        }

        return RateLimitStatus.Ok;
    }

    public async Task<int> GetRemainingAttemptsAsync(string email, CancellationToken cancellationToken)
    {
        var windowStart = DateTimeOffset.UtcNow.AddMinutes(-1);

        var filter = Builders<LoginAttemptRecord>.Filter.And(
            Builders<LoginAttemptRecord>.Filter.Eq(r => r.Email, email),
            Builders<LoginAttemptRecord>.Filter.Gte(r => r.Timestamp, windowStart),
            Builders<LoginAttemptRecord>.Filter.Eq(r => r.Success, false));

        var recentFailures = await _loginAttempts.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        return Math.Max(0, 5 - (int)recentFailures);
    }
}

public record LoginAttemptRecord(Guid Id, string Email, DateTimeOffset Timestamp, bool Success);

public enum RateLimitStatus
{
    Ok,
    RateLimited,
    LockedOut
}
