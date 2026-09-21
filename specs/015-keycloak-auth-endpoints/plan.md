# Implementation Plan: Keycloak Authentication Endpoints

**Branch**: `015-keycloak-auth-endpoints` | **Date**: 2026-09-20 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/015-keycloak-auth-endpoints/spec.md`

## Summary

Create Minimal API endpoints for user authentication (registration, login, token refresh, password reset) that delegate all account management operations to Keycloak via their Admin REST API. The API acts as a thin delegation layer — no local user storage required. Endpoints follow RFC 7807 Problem Details format and emit OpenTelemetry spans for observability.

## Technical Context

**Language/Version**: C# / .NET 10 (net10.0)

**Primary Dependencies**: 
- `Microsoft.AspNetCore.Authentication.JwtBearer` (JWT token validation)
- `FluentValidation` (request validation)
- `OpenTelemetry.*` packages (distributed tracing, metrics, logs)
- `MongoDB.Driver` (runtime config storage for Keycloak connection details)
- `System.Text.Json` (HTTP client serialization)

**Storage**: MongoDB stores runtime configuration (`KeycloakConfig` with `_id: "keycloak"`). No local user account storage — Keycloak is the source of truth.

**Testing**: TUnit framework + Testcontainers.Keycloak for integration tests; NSubstitute for unit test mocking

**Target Platform**: Linux server (Docker/Podman containerized deployment)

**Project Type**: Web service (ASP.NET Core Minimal API with vertical slice architecture)

**Performance Goals**: Registration < 5s, Login < 3s, Token refresh < 2s (end-to-end including Keycloak response time); 95% of auth requests respond within 10 seconds

**Constraints**: Rate limiting on login attempts (5 failures/minute, lockout after 10 failures in 15 minutes); Result pattern for error handling; RFC 7807 Problem Details format

**Scale/Scope**: Initial v1 authentication feature covering registration, login, token refresh, and password reset flows

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

| Principle | Status | Notes |
|-----------|--------|-------|
| I. SOLID & Separation of Concerns | ✅ Pass | Domain models in `Shelfly.Common` remain framework-agnostic; entity models in `Data/Entities/` handle persistence mapping |
| II. Vertical Slice Architecture | ✅ Pass | Auth endpoints co-located with validators, services, and DTOs under feature boundary |
| III. MVVM Pattern (Client) | ⏸ N/A | Primarily server-side implementation; client integration deferred to separate feature |
| IV. Coding Standards | ✅ Pass | Explicit types, collection expressions, primary constructors, Result pattern enforced |
| V. Data Management | ⏸ N/A | No local user storage; Keycloak manages accounts directly |
| VI. API Design & Versioning | ✅ Pass | URL versioning (`/v1/auth/*`), Minimal APIs, RFC 7807 errors |
| VII. Authentication & User Management | ✅ Pass | Keycloak delegation pattern aligns with constitution principle |
| VIII. Localization & Internationalization | ⏸ N/A | Server-side feature; localization applies to client UI text |
| IX. Asset & Resource Formats | ⏸ N/A | Not applicable for API endpoints |
| X. Microsoft Documentation Sourcing | ✅ Pass | OpenTelemetry configuration verified via microsoft-learn MCP server |
| XI. IDE-Assisted Refactoring | ✅ Pass | Rider MCP tools available for structural changes |

## Project Structure

### Documentation (this feature)

```text
specs/015-keycloak-auth-endpoints/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

```text
Shelfly.Api/
├── Features/
│   └── Auth/                    # Vertical slice for authentication feature
│       ├── Endpoints/           # Minimal API endpoint definitions
│       │   ├── RegisterEndpoint.cs
│       │   ├── LoginEndpoint.cs
│       │   ├── RefreshEndpoint.cs
│       │   └── ResetPasswordEndpoint.cs
│       ├── Services/            # Keycloak delegation services
│       │   ├── IAuthService.cs  # Interface for auth operations
│       │   └── AuthService.cs   # Implementation delegating to Keycloak
│       ├── Validators/          # FluentValidation request validators
│       │   ├── RegisterRequestValidator.cs
│       │   ├── LoginRequestValidator.cs
│       │   ├── RefreshRequestValidator.cs
│       │   └── ResetPasswordRequestValidator.cs
│       ├── DTOs/                # Request/response data transfer objects
│       │   ├── RegisterRequestDto.cs
│       │   ├── LoginRequestDto.cs
│       │   ├── RefreshRequestDto.cs
│       │   ├── ResetPasswordRequestDto.cs
│       │   └── AuthResponseDto.cs
│       └── Results/             # Result pattern implementations
│           ├── AuthResult.cs    # Base result type
│           └── RegisterResult.cs, LoginResult.cs, etc.
├── Data/
│   └── Entities/                # Persistence-specific entity models (if needed)
├── Extensions/                  # Extension methods for endpoint registration
│   └── AuthEndpointExtensions.cs
└── Program.cs                   # Entry point with service registrations

Shelfly.Api.Tests/
├── Integration/                 # Testcontainers-based integration tests
│   └── AuthEndpointsTests.cs    # Tests against real Keycloak container
└── Unit/                        # TUnit unit tests
    ├── AuthServiceTests.cs      # Mocked Keycloak client tests
    └── ValidatorTests.cs        # FluentValidation rule tests
```

**Structure Decision**: Vertical slice architecture under `Shelfly.Api/Features/Auth/` keeps all authentication concerns co-located. The API delegates to Keycloak via their Admin REST API — no local user storage required, so entity models are minimal (only runtime config in MongoDB). Extension methods in `Extensions/` provide clean endpoint registration syntax following C# 14 patterns.

## Complexity Tracking

> **Fill ONLY if Constitution Check has violations that must be justified**

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| OpenTelemetry spans per endpoint | FR-014 requires full distributed tracing for auth events | Simple logging insufficient for correlation across Keycloak boundary |
| Rate limiting middleware | FR-011 requires configurable lockout thresholds | Basic counter in service lacks persistence and thread safety |
