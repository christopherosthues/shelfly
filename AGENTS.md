# AGENTS.md

## Overview

Shelfly is a reading companion application that helps users manage bookmarks for physical books. Users can track their reading progress, save important passages, and organize their personal library across devices. The core domain revolves around Books (physical book records) and Bookmarks (saved page references with notes).

## Project Structure

Three-project .NET 10 solution (`Shelfly.slnx`):

| Project | Type | Framework | Role |
|---------|------|-----------|------|
| `Shelfly.Api` | ASP.NET Core Web API | net10.0 | Backend service — single entry point for all client traffic |
| `Shelfly.App` | .NET MAUI client | multi-target | Cross-platform mobile/desktop app, talks only to the API |
| `Shelfly.Common` | Class library | net10.0 | Shared domain models (Book, Bookmark) |

Both Api and App reference Common. No test projects exist yet.

## Commands

```bash
# Build solution
dotnet build Shelfly.slnx

# Run API locally (requires Keycloak config in appsettings.json)
dotnet run --project Shelfly.Api

# Docker compose (API only)
docker compose up
```

## Key Quirks

- **Centralized packages**: All NuGet versions pinned in `Directory.Packages.props`. Add new packages there, not in individual `.csproj` files.
- **MAUI target frameworks are conditional**: App targets vary by host OS (`net10.0-android` always; iOS/MacCatalyst on non-Linux; Windows only on Windows). Use `dotnet build Shelfly.App/Shelfly.App.csproj` to let MSBuild resolve conditionally.
- **XAML source generation enabled**: `<MauiXamlInflator>SourceGen</MauiXamlInflator>` generates C# from XAML at compile time.
- **global.json allows prerelease SDKs**: `<allowPrerelease>true</allowPrerelease>` with `rollForward: latestMajor`.
- **Request validation**: FluentValidation used for request data validation in the API

## Architecture Notes

- **Data stores**: PostgreSQL for primary data (EF Core + Npgsql), MongoDB for configuration parameters
- **Auth flow**: Keycloak handles authentication/authorization. The API delegates auth to Keycloak; the MAUI client communicates only with the API and never talks directly to Keycloak
- **Minimal hosting model**: single `Program.cs` entry point, endpoints defined via `app.MapGet()` etc., not Controllers
- **API surface**: The API exposes both a REST API and a GraphQL API for client consumption
- **Entity models** live in `Shelfly.Api/Data/Entities/`, separate from Common domain classes

## Testing Stack

- **Unit testing**: TUnit framework for unit tests across all test projects
- **Assertions**: Shouldly used for readable, natural-language assertions in test cases
- **Integration testing**: Testcontainers spins up isolated MongoDB, Keycloak, and PostgreSQL instances for integration tests

## .specify Integration

The `.specify/` directory contains workflow configuration for the specify toolchain (SDD cycle: specify → plan → tasks → implement). Not directly relevant to code changes, but explains the project's development process.
