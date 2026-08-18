<!-- Sync Impact Report:
  Version change: 2.1.0 → 2.2.0 (MINOR - new governance rule added)
  Modified principles:
    IV. Coding Standards (expanded with Result pattern mandate for error handling)
  Added sections: N/A
  Removed sections: N/A
  Changes: Error handling now MUST use Result pattern instead of custom/domain exceptions, making failure paths explicit at compile time
  Deferred TODOs: N/A
-->

# Shelfly Constitution

## Core Principles

### I. SOLID & Separation of Concerns

All code MUST adhere strictly to SOLID principles across every layer. Domain models in `Shelfly.Common` remain framework-agnostic and contain only business logic — no EF Core attributes, no controller dependencies, no UI concerns. Entity models in `Shelfly.Api/Data/Entities/` handle persistence-specific mapping and MAY include data annotations or FluentAPI configurations. The API layer MUST validate all incoming request data via FluentValidation before processing. NuGet packages MUST be centralized through `Directory.Packages.props`; individual `.csproj` files declare package references only, never versions.

**Rationale**: Separating Common domain models from Api entity models enables independent evolution of business logic and persistence layers. Centralized package management prevents version drift across the three-project solution. FluentValidation at the boundary ensures predictable error handling before business logic executes.

### II. Vertical Slice Architecture

Project structure MUST follow features, not technical layers. Each feature or business capability (e.g., Books, Bookmarks) lives inside its own directory containing all concerns: models, services, endpoints, views, DTOs, and tests. The server follows **Modular Monolith** architecture — modules communicate via direct service calls, not event buses (event bus usage requires explicit approval). Each vertical slice is self-contained and independently navigable. Shared infrastructure (common utilities, base classes) MAY reside in a dedicated shared directory, but feature-specific code MUST be co-located within the feature boundary.

**Rationale**: Feature-based organization improves discoverability and reduces cognitive load when navigating the codebase. Co-locating related code enables faster feature isolation, easier refactoring, and clearer ownership boundaries. Direct service calls between modules reduce indirection and simplify debugging compared to event-driven communication.

### III. MVVM Pattern (Client)

The MAUI client (`Shelfly.App`) MUST follow MVVM pattern with Shell navigation. Pages and ViewModels MUST be registered via `AddScopedWithShellRoute<TPage, TViewModel>("route")` for DI registration, enabling constructor injection on both pages and view models. Use CommunityToolkit patterns: `ObservableObject`, `ObservableProperty`, `RelayCommand`. XAML views MUST leverage source generation (`MauiXamlInflator=SourceGen`) for compile-time validation. The client communicates exclusively with the API — no direct Keycloak calls — ensuring consistent authentication flows across platforms (Android always, iOS/MacCatalyst on non-Linux, Windows conditionally).

**Rationale**: Scoped DI registration tied to Shell routes ensures view models are recreated per navigation, preventing stale state. XAML source generation catches binding errors at compile time rather than runtime. Single API dependency keeps the client layer thin and simplifies auth flow changes.

### IV. Coding Standards

Type inference (`var`) MUST be avoided except for complete anonymous types (`new { ... }`). Prefer explicit type names to improve readability and maintainability. Collections MUST use collection expression syntax (e.g., `[1, 2, 3]`) over `new List<T>()` or `new T[]()`. Constructors MUST prefer primary constructor syntax where applicable (e.g., `class Book(string title) { }`). Object instantiation MUST prefer `new()` syntax (C# 12) over explicit constructor calls when default constructors are used. Nullable reference types (`<Nullable>enable</Nullable>`) enforced across all projects — explicit `?` annotations required for nullable parameters and return types. Naming conventions follow standard .NET patterns, enforced via `.editorconfig`. No custom or domain-specific exceptions SHOULD be created or thrown; instead, the **Result pattern** MUST be used to represent success/failure outcomes explicitly in method signatures.

**Rationale**: Explicit typing reduces ambiguity in complex expressions. Collection expressions and primary constructors reduce boilerplate without sacrificing clarity. Nullable enforcement prevents null-reference runtime exceptions. Consistent naming conventions improve team readability and IDE navigation. The Result pattern makes error handling explicit at the call site, eliminates hidden control flow via exception propagation, and enables compile-time verification of failure paths — reducing runtime surprises and improving testability across all layers.

### V. Data Management

Books use soft deletion via nullable `DeletedAt` timestamp (`null` = active, non-null = deleted); queries MUST filter out records where `DeletedAt != null` unless explicitly requested. Bookmarks use hard deletion — physically removed from storage only when the parent book is also hard deleted. When a parent entity is hard-deleted, all dependent child entities MUST cascade delete automatically (e.g., bookmarks deleted when their book is removed). EF Core inherently implements Repository and UnitOfWork patterns through `DbContext`; custom repositories are unnecessary unless reading from multiple distinct sources (e.g., Database + Filesystem). Different tables in the same database ≠ multiple sources. Client-server bookmark synchronization uses **last-write-wins** based on `lastModified` timestamp. All entity identifiers MUST use UUID version 7 (`Guid.CreateVersion7()`) for time-ordered generation, enabling efficient sorting and indexing across databases.

**Rationale**: A deletion date eliminates confusion between soft delete (recoverable trash) and hard delete (permanent removal). Physical row deletion ensures storage efficiency. Leveraging EF Core's native patterns avoids redundant abstraction layers. Last-write-wins provides predictable conflict resolution for cross-device synchronization. UUID version 7 produces time-ordered identifiers that improve database index locality, reduce page splits, and enable natural chronological sorting without additional timestamp columns.

### VI. API Design & Versioning

REST endpoints MUST use URL versioning (e.g., `/v1/books`, `/v1/bookmarks`). Minimal APIs preferred — single `Program.cs` entry point with endpoints defined via `app.MapGet()` patterns, not Controllers. Error responses MUST follow **RFC 7807 Problem Details** format. GraphQL schema design is **Code-first** (C# classes → schema). Server uses HotChocolate; Client uses StrawberryShake for type-safe queries. Both REST and GraphQL APIs share the same underlying domain models from `Shelfly.Common`.

**Rationale**: URL versioning provides clear backward-compatibility boundaries. Minimal API design reduces boilerplate and improves endpoint discoverability. RFC 7807 standardizes error responses across all clients. Code-first GraphQL keeps schema definitions in C#, enabling compile-time validation and reducing drift between code and schema.

### VII. Authentication & User Management

Keycloak handles user registration, login, profile management, and token-based auth/authz. The API delegates authentication to Keycloak — validating JWT tokens against the configured issuer using JWKS discovery. Custom `JwtAudienceValidator` validates JWT `aud` claim against configured audience; mismatch returns 401 Unauthorized. Role-based access rules stored in MongoDB define role-to-endpoint mappings; the API enforces these policies at runtime. The client app configures server URL dynamically — no central server assumption. Keycloak configuration cached in-memory with 5-minute TTL to reduce MongoDB read frequency. Admin can update Keycloak configuration and authorization rules without restarting the API service.

**Rationale**: Delegating auth to Keycloak keeps authentication logic centralized and auditable. In-memory caching prevents redundant MongoDB reads on every request. Runtime refresh enables operational flexibility without downtime. Dynamic server URL supports self-hosted deployments where users control their own infrastructure.

## Testing & Observability

### Testing Strategy

Unit tests MUST use TUnit framework with Shouldly for readable, natural-language assertions. Integration tests MUST use TestContainers to spin up isolated PostgreSQL, MongoDB, and Keycloak instances. Unit tests cover all FluentValidation rules, domain model invariants, and business logic paths. Integration tests verify API endpoints against the PostgreSQL data store using EF Core test harnesses. The Red-Green-Refactor cycle is enforced: tests written → approved → failing → implemented → passing. No code merges without green test suites. Test projects MUST exist for all projects before feature implementation begins.

**Rationale**: TUnit provides modern, attribute-free test definitions that integrate cleanly with .NET 10. Shouldly assertions produce readable failure messages, reducing debugging time. TestContainers provide isolated, reproducible environments for integration verification, catching EF Core mapping issues and database constraint violations early.

### Logging & Observability

Client logging MUST use structured logging via NLog integrated with `Microsoft.Extensions.Logging`. Logs written to local files on the device. Server logging MUST use structured logging via `Microsoft.Extensions.Logging` (console output in development, file output in production). All log entries MUST include timestamp, severity level, and contextual data (user ID, request ID where applicable). Sensitive data (tokens, passwords) MUST be masked or excluded from logs unless explicitly approved for debugging.

**Rationale**: Structured logging enables efficient filtering and aggregation across distributed components. Local file storage on the client preserves diagnostic history without network dependency. Server-side structured output supports container orchestration tools that parse log streams. Consistent contextual data enables request tracing across API boundaries.

## DevOps & Configuration

### Containerization & Deployment

Use Dockerfiles and `compose.yaml` for both development and production environments. Execution engine: Podman (not Docker). The existing `compose.yaml` defines services for the API, PostgreSQL, MongoDB, Keycloak, and pgAdmin. A dedicated migration service/utility Docker container manages database schema changes separately from the main application stack. Migration containers integrate with the main stack by connecting to the same PostgreSQL instance defined in `compose.yaml`. Services MUST be named consistently across compose files and environment files to enable automatic variable resolution.

**Rationale**: Podman provides daemonless container execution, improving security and resource efficiency. Separating migrations into a dedicated service enables atomic schema changes without application downtime. Consistent naming conventions simplify environment management and reduce configuration errors.

### Configuration & Secrets Management

Per-service configuration managed via separate `<service>.env` files stored in `envfiles/` directory (e.g., `shelfly.env`, `postgres.env`, `keycloak.env`). Podman Compose loads these files automatically based on service names. MongoDB currently stores runtime configuration parameters and secrets ("for the moment") — including KeycloakConfig (`_id: "keycloak"`), PostgreSqlConfig (`_id: "postgresql"`), and AuthorizationRule (`_id: "auth-rules"`). Default configuration seeded before first API startup by an admin using a separate console program. MongoDB connection wrapped with Polly retry policy (exponential backoff, max 5 attempts) for resilience. This approach is temporary; a clear migration path to dedicated secrets management (e.g., HashiCorp Vault, Kubernetes Secrets) is expected as the project matures. Services MUST read from `.env` files for infrastructure configuration and MongoDB for runtime parameters until migration occurs.

**Rationale**: Per-service environment files enable granular configuration control without cross-service coupling. MongoDB provides a centralized, queryable store for runtime parameters that can be updated without restarts. Polly retry policy ensures graceful degradation during transient MongoDB failures. Temporary nature acknowledged with explicit migration expectation to prevent long-term technical debt.

## Workflow & Dependency Policy

### Process & Workflow

Commits MUST follow Conventional Commits specification (`feat:`, `fix:`, `chore:`, `docs:`, etc.). Versioning follows Semantic Versioning (SemVer) for releases — MAJOR.MINOR.PATCH format with clear backward-compatibility boundaries. CI/CD pipeline via GitHub Actions MUST include stages: build, test, lint, publish, deploy. All feature work MUST follow the Spec Kit SDD cycle: specify → plan → tasks → implement. Pull requests MUST include updated tests and pass `dotnet build Shelfly.slnx` successfully before merge.

**Rationale**: Conventional commits enable automated changelog generation and semantic versioning. Structured CI/CD stages ensure quality gates are enforced consistently. The SDD cycle ensures features are fully specified before implementation begins, reducing rework. Requiring successful solution builds on PR merge prevents integration breakage across the three interdependent projects.

### Dependency Policy

No additional NuGet packages or libraries added without explicit approval. Always ask before adding new dependencies to the solution. Approved dependencies MUST be documented in `Directory.Packages.props` with version pinning and rationale comments. Transitive dependencies reviewed during each major release cycle for security vulnerabilities and compatibility issues. Third-party libraries MUST support .NET 10 targeting and maintain active development status (last release within 12 months).

**Rationale**: Explicit approval prevents dependency sprawl and reduces attack surface. Centralized version pinning ensures consistency across projects. Regular vulnerability reviews catch security issues before they become critical. Active maintenance requirement reduces long-term compatibility risks.

## Governance

This constitution supersedes all other development practices and conventions for the Shelfly project. Amendments require documentation of the change rationale, stakeholder approval, and a migration plan if existing code is affected. All pull requests and code reviews MUST verify compliance with the active principles. Complexity additions (new dependencies, architectural patterns) MUST be justified in writing against the relevant principle. Use `AGENTS.md` for runtime development guidance on project structure, commands, and quirks.

**Version**: 2.2.0 | **Ratified**: 2026-08-13 | **Last Amended**: 2026-08-18
