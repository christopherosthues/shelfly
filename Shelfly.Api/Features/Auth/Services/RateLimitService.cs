using MongoDB.Driver;

namespace Shelfly.Api.Features.Auth.Services;

public class RateLimitService(IMongoDatabase mongoDatabase, ILogger<RateLimitService> logger)
{
    private readonly IMongoCollection<LoginAttemptRecord> _loginAttempts = mongoDatabase.GetCollection<LoginAttemptRecord>("login_attempts");

    public async Task<bool> IsLockedOutAsync(string email, CancellationToken cancellationToken)
    {
        DateTimeOffset windowStart = DateTimeOffset.UtcNow.AddMinutes(-15);

        FilterDefinition<LoginAttemptRecord> filter = Builders<LoginAttemptRecord>.Filter.And(
            Builders<LoginAttemptRecord>.Filter.Eq(r => r.Email, email),
            Builders<LoginAttemptRecord>.Filter.Gte(r => r.Timestamp, windowStart));

        long count = await _loginAttempts.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

        bool lockedOut = count >= 10;

        if (lockedOut)
        {
            logger.LogWarning("Account {Email} temporarily locked after {Count} failures in 15 minutes", email, count);
        }

        return lockedOut;
    }

    public async Task<RateLimitStatus> RecordLoginAttemptAsync(string email, bool success, CancellationToken cancellationToken)
    {
        LoginAttemptRecord record = new LoginAttemptRecord(Guid.CreateVersion7(), email, DateTimeOffset.UtcNow, success);

        await _loginAttempts.InsertOneAsync(record, cancellationToken: cancellationToken);

        if (!success)
        {
            DateTimeOffset windowStart = DateTimeOffset.UtcNow.AddMinutes(-1);

            FilterDefinition<LoginAttemptRecord> filter = Builders<LoginAttemptRecord>.Filter.And(
                Builders<LoginAttemptRecord>.Filter.Eq(r => r.Email, email),
                Builders<LoginAttemptRecord>.Filter.Gte(r => r.Timestamp, windowStart),
                Builders<LoginAttemptRecord>.Filter.Eq(r => r.Success, false));

            long recentFailures = await _loginAttempts.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

            if (recentFailures >= 5)
            {
                return RateLimitStatus.RateLimited;
            }
        }

        return RateLimitStatus.Ok;
    }

    public async Task<int> GetRemainingAttemptsAsync(string email, CancellationToken cancellationToken)
    {
        DateTimeOffset windowStart = DateTimeOffset.UtcNow.AddMinutes(-1);

        FilterDefinition<LoginAttemptRecord> filter = Builders<LoginAttemptRecord>.Filter.And(
            Builders<LoginAttemptRecord>.Filter.Eq(r => r.Email, email),
            Builders<LoginAttemptRecord>.Filter.Gte(r => r.Timestamp, windowStart),
            Builders<LoginAttemptRecord>.Filter.Eq(r => r.Success, false));

        long recentFailures = await _loginAttempts.CountDocumentsAsync(filter, cancellationToken: cancellationToken);

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
