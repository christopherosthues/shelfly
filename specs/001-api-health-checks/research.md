# Research: API Health Checks

**Date**: 2026-09-21

## Decisions and Rationale

### Decision 1: Health Check Package Selection

**Decision**: Use `Microsoft.Extensions.Diagnostics.HealthChecks` (core framework package)

**Rationale**: 
- Official ASP.NET Core health check infrastructure with built-in middleware, DI integration, and standardized HTTP status mapping
- Supports named checks, tags for endpoint filtering, custom response writers, and timeout configuration
- Provides `HealthCheckService` for programmatic execution and `MapHealthChecks()` for endpoint registration

**Alternatives considered**:
- `AspNetCore.Diagnostics.HealthChecks` (community package): Offers pre-built checks for MongoDB/PostgreSQL but introduces external dependency; the constitution requires explicit approval for new packages, and custom implementations are straightforward using existing drivers
- Lambda-based endpoint handlers: Simpler but lack structured timeout management, dependency injection support, and standardized health status reporting

### Decision 2: Endpoint URL Structure

**Decision**: Separate endpoints — `/v1/health/live` (liveness) and `/v1/health/ready` (readiness)

**Rationale**:
- Aligns with Kubernetes/Podman probe semantics: liveness for process verification, readiness for dependency validation
- Enables independent configuration (different timeouts, different check sets)
- Follows industry standard patterns (e.g., eShopOnContainers uses `/hc` for combined checks; separating provides clearer failure attribution)

**Alternatives considered**:
- Single endpoint with query parameter (`/v1/health?type=live|ready`): Adds parsing complexity and couples two distinct concerns
- Combined endpoint with tags: Requires predicate filtering on every request, increasing latency

### Decision 3: Dependency Check Implementation Strategy

**Decision**: Custom `IHealthCheck` implementations using existing drivers (MongoDB.Driver ping command, Npgsql connection + query)

**Rationale**:
- MongoDB: `ping` command on admin database is the standard connectivity verification; lightweight and reliable
- PostgreSQL: Simple `SELECT 1` query validates both network connectivity and database responsiveness
- Keycloak: HTTP GET to `/realms/{realm}` endpoint verifies auth service availability
- All checks use existing connection strings from configuration — no new infrastructure required

**Alternatives considered**:
- Pre-built health check packages (e.g., `AspNetCore.HealthChecks.MongoDb`): Adds package dependency; custom implementation is 20 lines of code
- DbContext-based PostgreSQL check: Requires EF Core DbContext registration; raw Npgsql connection is lighter and available earlier in startup

### Decision 4: Response Format

**Decision**: Custom JSON response via `HealthCheckOptions.ResponseWriter` with dependency name, status, and failure category

**Rationale**:
- Satisfies FR-007 (categorized summary without raw errors)
- Enables monitoring dashboards to parse structured data for alerting rules
- Failure categories ("timeout", "connection refused") provide actionable troubleshooting information
- Uses `System.Text.Json` for serialization — no additional package required

**Alternatives considered**:
- Default plaintext response: Minimal but lacks structured data for automated alerting
- Full error details with stack traces: Exposes internal topology and implementation details to orchestrators

### Decision 5: Timeout Configuration

**Decision**: 3 seconds per dependency (clarified in spec session)

**Rationale**:
- Standard threshold for database connectivity probes; balances timely failure detection against false positives from transient network latency
- Total readiness endpoint latency capped at 10 seconds (3s × 3 dependencies + overhead)
- Aligns with SC-002 measurable outcome

## Best Practices Applied

1. **Health Check Registration**: Use `AddCheck<T>()` with named checks and tags for endpoint filtering
2. **Timeout Management**: Configure per-check timeout via `HealthCheckRegistration.Timeout` property
3. **Failure Status Mapping**: Map `HealthStatus.Unhealthy` to HTTP 503 via `ResultStatusCodes`
4. **Cancellation Support**: All async checks accept `CancellationToken` for graceful shutdown
5. **Test Isolation**: Integration tests use Testcontainers for real database instances; unit tests mock driver interfaces

## Package Addition Required

**Package**: `Microsoft.Extensions.Diagnostics.HealthChecks`

**Version**: 10.0.12 (aligned with .NET 10.0 framework packages)

**Rationale**: Core health check infrastructure — no functionality overlap with existing packages. Required for `AddHealthChecks()`, `MapHealthChecks()`, and `IHealthCheck` interface.
