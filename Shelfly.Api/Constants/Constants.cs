namespace Shelfly.Api.Constants;

internal static class ActivitySources
{
    public const string Api = "shelfly-api";
    public const string HealthChecks = "shelfly-health-checks";
    public const string ConfigProvider = "shelfly-config-provider";
}

internal static class TagKeys
{
    public static class HealthCheck
    {
        public const string Name = "health.check.name";
        public const string Status = "health.check.status";
        public const string Exception = "health.check.exception";
        public const string ResponseStatusCode = "http.response_status_code";
    }

    public static class Config
    {
        public const string Operation = "config.operation";
        public const string DocFound = "config.doc_found";
        public const string Exception = "config.exception";
        public const string Deserialized = "config.deserialized";
        public const string Updated = "config.updated";
        public const string Canceled = "config.canceled";
    }

    public const string Operation = "operation";
}

internal static class HealthCheckNames
{
    public const string MongoDb = "mongodb";
    public const string PostgreSql = "postgresql";
    public const string Keycloak = "keycloak";
    public const string Liveness = "liveness";
}

internal static class HealthCheckTags
{
    public const string Live = "live";
    public const string Ready = "ready";
}

internal static class HealthStatusValues
{
    public const string Healthy = "Healthy";
    public const string Unhealthy = "Unhealthy";
}

internal static class OperationNames
{
    public const string Load = "load";
    public const string Poll = "poll";
}

internal static class MongoDbConstants
{
    public const string AdminDatabase = "admin";
    public const string ShelflyDatabase = "shelfly";
    public const string ServerConfigurationCollection = "server_configuration";
    public const string GlobalConfigDocumentId = "global_config";
    public const string PingCommand = "ping";
}

internal static class MetricNames
{
    public const string FailedRequestsCounter = "shelfly.config.failed_requests";
    public const string PollDurationHistogram = "shelfly.config.poll_duration";
    public const string CountUnit = "count";
    public const string MsUnit = "ms";
    public const string FailedRequestsDescription = "Number of failed MongoDB configuration retrievals";
    public const string PollDurationDescription = "Duration of MongoDB configuration poll operations";
}
