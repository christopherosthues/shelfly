<!-- Sync Impact Report:
  Version change: 1.0.2 → 1.0.3 (PATCH - added vertical slice architecture principle)
  Modified principles: N/A
  Added sections: VII. Vertical Slice Architecture
  Removed sections: N/A
  Changes: Core Principles - added feature-based project structure mandate with vertical slice guidance
  Deferred TODOs: N/A
-->

# Shelfly Constitution

## Core Principles

### I. Code Quality

All code MUST adhere to SOLID principles and maintain separation of concerns across the three-project architecture. Domain models in `Shelfly.Common` remain framework-agnostic; entity models in `Shelfly.Api/Data/Entities/` handle persistence-specific concerns. FluentValidation MUST validate all incoming request data before processing. NuGet packages MUST be centralized via `Directory.Packages.props` — individual `.csproj` files only declare package references, not versions. Code reviews MUST verify that new endpoints follow the minimal hosting model (`app.MapGet()` patterns) rather than introducing Controllers unless justified by complexity.

**Rationale**: Centralized package management prevents version drift across projects. Separating Common domain models from Api entity models enables independent evolution of business logic and persistence layers. Minimal API design reduces boilerplate and improves endpoint discoverability.

### II. Testing Standards (NON-NEGOTIABLE)

Test projects MUST exist for `Shelfly.Api` and `Shelfly.Common` before feature implementation begins. Unit tests MUST cover all FluentValidation rules, domain model invariants, and business logic paths. Integration tests MUST verify API endpoints against the PostgreSQL data store using EF Core test harnesses. The Red-Green-Refactor cycle is enforced: tests written → approved → failing → implemented → passing. No code merges without green test suites.

**Rationale**: With no test projects currently existing, establishing test-first discipline prevents technical debt accumulation. API endpoints backed by PostgreSQL require integration verification beyond unit tests to catch EF Core mapping issues and database constraint violations early.

### III. User Experience Consistency

The MAUI client (`Shelfly.App`) MUST deliver visually consistent experiences across all target platforms (Android always, iOS/MacCatalyst on non-Linux, Windows conditionally). XAML views MUST leverage source generation (`MauiXamlInflator=SourceGen`) for compile-time validation. Shared UI components MUST reside in a common resource dictionary or shared view library to prevent platform-specific divergence. The client communicates exclusively with the API — no direct Keycloak calls — ensuring consistent authentication flows and error handling across platforms.

**Rationale**: Cross-platform consistency reduces user confusion and support burden. XAML source generation catches binding errors at compile time rather than runtime. Single API dependency simplifies auth flow changes and keeps the client layer thin.

### IV. Performance Requirements

API endpoints MUST respond within 200ms for p95 latency under normal load. Database queries using EF Core MUST include explicit indexing strategies for frequently queried fields. MongoDB configuration parameters MUST be cached in-memory with TTL-based invalidation to reduce read latency. Batch operations (e.g., bulk book imports) MUST use asynchronous streaming patterns and report progress via SignalR or similar real-time channels. Connection pooling MUST be configured for both PostgreSQL and MongoDB clients.

**Rationale**: Shelfly serves as a reading companion application where responsiveness directly impacts user engagement. EF Core queries without proper indexing degrade quickly as the library catalog grows. Configuration caching prevents redundant MongoDB reads on every request.

### V. Dependency Injection Preference

All service dependencies MUST be injected via constructor dependency injection. The DI container controls object lifetimes, ensuring consistent lifecycle management and testability. Parameterless constructors SHOULD only exist for setup/bootstrap scenarios where the DI container is not yet available (e.g., config service needed before container build). EF Core already implements the Repository and UnitOfWork patterns natively through `DbContext` — separate repository abstractions are unnecessary unless data is loaded from different sources (e.g., database combined with file system or external APIs).

**Rationale**: Constructor injection enables dependency mocking during unit tests, reduces coupling between components, and prevents stale references. Leveraging EF Core's native patterns avoids redundant abstraction layers and simplifies the persistence architecture. Custom repositories are justified only when aggregating data from heterogeneous sources.

### VI. Data Lifecycle & Deletion Patterns

Entity lifecycle states MUST use a two-value enum: `Active` (normal use) and `SoftDeleted` (in trash, recoverable). Soft deletion marks an entity as trashed without removing it from storage, enabling user recovery. Hard deletion physically removes the entity row from the database—hard-deleted entities no longer exist in storage rather than being marked with a status flag. When a parent entity is hard-deleted, all dependent child entities MUST cascade delete automatically (e.g., bookmarks deleted when their book is removed).

**Rationale**: A two-state enum eliminates confusion between soft delete (recoverable trash) and hard delete (permanent removal). Physical row deletion ensures storage efficiency and prevents phantom records. Cascade delete maintains referential integrity without orphaned child records.

### VII. Vertical Slice Architecture

Project structure MUST follow features, not technical layers. Everything related to a feature (e.g., Books, Bookmarks) lives inside its own directory containing all concerns: models, services, endpoints, views, and tests. Each feature forms a vertical slice—self-contained and independently navigable. Shared infrastructure (common utilities, base classes) MAY reside in a dedicated shared directory, but feature-specific code MUST be co-located within the feature boundary.

**Rationale**: Feature-based organization improves discoverability and reduces cognitive load when navigating the codebase. Co-locating related code enables faster feature isolation, easier refactoring, and clearer ownership boundaries. Vertical slices support independent development, testing, and deployment of features without cross-cutting layer dependencies.

## Architecture & Technology Constraints

The solution targets .NET 10 (`net10.0`) with prerelease SDK tolerance enabled via `global.json`. The API layer uses ASP.NET Core minimal hosting with a single `Program.cs` entry point. Authentication delegates to Keycloak; the API validates tokens using `VerifyUserHasAnyAcceptedScope()`. Data persistence splits between PostgreSQL (primary data via EF Core + Npgsql) and MongoDB (configuration parameters). MAUI target frameworks resolve conditionally — build scripts MUST use `dotnet build Shelfly.App/Shelfly.App.csproj` to allow MSBuild conditional evaluation.

**Rationale**: The dual-data-store architecture separates mutable user content from stable configuration, enabling independent scaling. Keycloak delegation keeps auth logic centralized and auditable. Conditional MAUI targeting prevents build failures on platforms without the required SDK tooling installed.

## Development Workflow

All feature work MUST follow the Spec Kit SDD cycle: specify → plan → tasks → implement. The `.specify/` directory contains workflow configuration driving this process. Code changes MUST be committed with descriptive messages following conventional commit format (`feat:`, `fix:`, `docs:`, etc.). Docker Compose provides an alternative local runtime for the API service. Pull requests MUST include updated tests and pass `dotnet build Shelfly.slnx` successfully.

**Rationale**: The structured SDD cycle ensures features are fully specified before implementation begins, reducing rework. Conventional commits enable automated changelog generation and semantic versioning. Requiring successful solution builds on PR merge prevents integration breakage across the three interdependent projects.

## Governance

This constitution supersedes all other development practices and conventions for the Shelfly project. Amendments require documentation of the change rationale, stakeholder approval, and a migration plan if existing code is affected. All pull requests and code reviews MUST verify compliance with the active principles. Complexity additions (new dependencies, architectural patterns) MUST be justified in writing against the relevant principle. Use `AGENTS.md` for runtime development guidance on project structure, commands, and quirks.

**Version**: 1.0.3 | **Ratified**: 2026-08-13 | **Last Amended**: 2026-08-16
