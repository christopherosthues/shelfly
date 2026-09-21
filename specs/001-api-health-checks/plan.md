# Implementation Plan: API Health Checks

**Branch**: `[001-api-health-checks]` | **Date**: 2026-09-21 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/001-api-health-checks/spec.md`

## Summary

Add liveness and readiness health check endpoints to the Shelfly API using `Microsoft.Extensions.Diagnostics.HealthChecks`. The implementation provides separate endpoints for basic process verification (`/v1/health/live`) and dependency connectivity validation (`/v1/health/ready`) with structured JSON responses including categorized failure information. Custom `IHealthCheck` implementations validate PostgreSQL, MongoDB, and Keycloak connectivity with 3-second per-dependency timeouts.

## Technical Context

**Language/Version**: C# / .NET 10.0

**Primary Dependencies**: 
- `Microsoft.Extensions.Diagnostics.HealthChecks` (new package required)
- Existing: `MongoDB.Driver` 3.12.0, `Npgsql` 10.0.3

**Storage**: PostgreSQL (primary data), MongoDB (configuration store)

**Testing**: TUnit for unit tests; Testcontainers.PostgreSql and Testcontainers.MongoDb for integration tests

**Target Platform**: Linux server (Docker/Podman containerized), Windows development

**Project Type**: Web service (ASP.NET Core Minimal API)

**Performance Goals**: Liveness <200ms, Readiness <10s total latency with 3s per-dependency timeout

**Constraints**: HTTP 503 on any single dependency failure; categorized error responses without raw messages

**Scale/Scope**: Single API service; health endpoints serve orchestrator probes (Kubernetes, Podman Compose)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-checked after Phase 1 design.*

| Principle | Compliance | Notes |
|-----------|------------|-------|
| I. SOLID & Separation of Concerns | Pass | Health checks isolated in feature directory; no domain model pollution |
| II. Vertical Slice Architecture | Pass | Feature co-located: endpoints, health checks, services under `Features/HealthChecks/` |
| IV. Coding Standards | Pass | Primary constructors, collection expressions, nullable reference types, Result pattern for error categorization |
| VI. API Design & Versioning | Pass | URL versioned (`/v1/health/*`); structured JSON responses per contract |
| VII. Authentication | Pass | Endpoints accessible without auth (FR-006) — justified for orchestrator probes |
| Testing Strategy | Pass | Unit tests for health check logic; integration tests with Testcontainers |

**Post-Design Re-evaluation**: All gates remain passing. The design introduces one new NuGet package (`Microsoft.Extensions.Diagnostics.HealthChecks` v10.0.12) documented in Complexity Tracking section — aligned with Dependency Policy requiring explicit approval.

## Project Structure

### Documentation (this feature)

```text
specs/001-api-health-checks/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (API endpoint contracts)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
Shelfly.Api/
├── Features/
│   └── HealthChecks/
│       ├── Endpoints/
│       │   └── HealthEndpointExtensions.cs    # Route registration for /v1/health/*
│       ├── Checks/
│       │   ├── LivenessHealthCheck.cs         # Basic process liveness check
│       │   ├── PostgreSQLHealthCheck.cs       # Database connectivity validation
│       │   ├── MongoDBHealthCheck.cs          # Config store connectivity validation
│       │   └── KeycloakHealthCheck.cs         # Auth service connectivity validation
│       ├── DTOs/
│       │   └── HealthCheckResponseDto.cs      # Structured response format
│       └── Services/
│           └── HealthCheckResultService.cs    # Result aggregation and categorization

Shelfly.Api.Tests/
├── Features/
│   └── HealthChecks/
│       ├── Unit/
│       │   ├── LivenessHealthCheckTests.cs
│       │   ├── PostgreSQLHealthCheckTests.cs
│       │   ├── MongoDBHealthCheckTests.cs
│       │   └── KeycloakHealthCheckTests.cs
│       └── Integration/
│           └── HealthEndpointIntegrationTests.cs  # Testcontainers-based tests
```

**Structure Decision**: Vertical slice under `Features/HealthChecks/` following the established pattern (see `Features/Auth/`). Health checks are self-contained with endpoints, implementations, DTOs, and services co-located. No cross-feature dependencies — health checks consume existing DI-registered services (`IMongoDatabase`, connection strings) without introducing new abstractions.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| New NuGet package (`Microsoft.Extensions.Diagnostics.HealthChecks`) | Core framework for structured health check registration and middleware | Lambda-based endpoint handlers lack dependency injection, timeout management, and standardized HTTP status mapping |
