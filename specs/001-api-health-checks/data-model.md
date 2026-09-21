# Data Model: API Health Checks

**Date**: 2026-09-21

## Entities

### HealthCheckResponseDto

Represents the structured health check response returned by both liveness and readiness endpoints.

| Field | Type | Description |
|-------|------|-------------|
| `Status` | `string` | Overall health status: "Healthy", "Degraded", or "Unhealthy" |
| `Dependencies` | `IEnumerable<DependencyStatusDto>` | Individual dependency check results (readiness only) |
| `Timestamp` | `DateTimeOffset` | Response generation time |

### DependencyStatusDto

Represents a single dependency health check result.

| Field | Type | Description |
|-------|------|-------------|
| `Name` | `string` | Dependency identifier: "PostgreSQL", "MongoDB", or "Keycloak" |
| `Status` | `string` | Check result: "Healthy" or "Unhealthy" |
| `FailureCategory` | `string?` | Failure classification when unhealthy: "timeout", "connection refused", or null when healthy |
| `Duration` | `TimeSpan` | Time elapsed for this check |

## Validation Rules

- `Status` must be one of: "Healthy", "Degraded", "Unhealthy"
- `FailureCategory` is nullable; required only when `Status` is "Unhealthy"
- `Dependencies` collection is empty for liveness checks (process-only verification)
- `Duration` values are non-negative

## State Transitions

Health check results follow this state machine:

```
Startup → Liveness Healthy (immediate)
         → Readiness Check (parallel dependency validation)
           → All healthy → HTTP 200 + "Healthy"
           → Any failed → HTTP 503 + "Unhealthy" with failure categories
```

## Data Volume Assumptions

- Health check responses are lightweight (<1KB JSON payload)
- No persistent storage required; results are ephemeral per-request
- Orchestrator probes typically poll at 10-30 second intervals
