# AGENTS.md

## Overview

Shelfly is a reading companion for managing bookmarks of physical books. Core domain: **Books** (book records) and **Bookmarks** (saved page references with notes).

## Project Structure

.NET 10 solution (`Shelfly.slnx`).

| Project | Type | Framework | Role |
|---------|------|-----------|------|
| `Shelfly.Api` | ASP.NET Core Web API | net10.0 | Backend; single entry point for all client traffic |
| `Shelfly.App` | .NET MAUI client | multi-target | Cross-platform app; talks only to the API |
| `Shelfly.App.Data` | Class library | net10.0 | App local persistence (EF Core + SQLite) |
| `Shelfly.App.Migrations` | Class library | net10.0 | EF Core migrations for `Shelfly.App.Data` |
| `Shelfly.Common` | Class library | net10.0 | Shared domain models (Book, Bookmark) |
| `Shelfly.Configuration` | Class library | net10.0 | Shared config types (placeholder, currently empty) |
| `Shelfly.AdminConsole` | .NET console app | net10.0 | CLI admin tool for API config (skeleton) |
| `*.Tests` (Api, App, Common, AdminConsole) | Test projects | net10.0 | TUnit unit + integration tests |

Project references: Api → Common, Configuration; App → Common, App.Data, App.Migrations; App.Migrations → App.Data; AdminConsole → Configuration.

Project-specific agent notes live next to each project — read the relevant file when working in that project:

| File | Contents |
|------|----------|
| `Shelfly.Api/AGENTS.md` | Architecture, Keycloak auth flow, MongoDB config storage, resilience |
| `Shelfly.App/AGENTS.md` | MAUI conditional targets, XAML source generation, app boundaries |
| `Shelfly.Common/AGENTS.md` | Domain model notes |

## Commands

```bash
dotnet build Shelfly.slnx                 # build solution
dotnet test Shelfly.slnx                  # run all tests
dotnet run --project Shelfly.Api          # run API (needs Keycloak config in appsettings.json)
docker compose up                          # API only
```

## Key Quirks

- **Centralized packages**: NuGet versions pinned in `Directory.Packages.props`; add packages there, not in `.csproj`.
- **global.json**: `<allowPrerelease>true</allowPrerelease>` with `rollForward: latestMajor`.

## Coding Standards

- **Nullable reference types** enabled everywhere; explicit `?` for nullable parameters/returns.
- **No `var`** unless the type is completely anonymous.
- **Primary constructors** preferred for classes/records to cut boilerplate.
- **C# 12 extension members** where they improve clarity without hiding implementation.
- **Collection expressions** (`[]`) over `new List<T>()`/`new T[]()`.
- **Concise `new()`** over explicit constructor calls for default constructors.

## Testing Stack

- **Unit**: TUnit. **Assertions**: Shouldly. **Mocking**: NSubstitute.
- **Integration**: Testcontainers (`Shelfly.Api.Tests`: isolated MongoDB, Keycloak, PostgreSQL; `Shelfly.AdminConsole.Tests`: MongoDB).

## .specify

`.specify/` holds workflow config for the specify toolchain (SDD cycle: specify → plan → tasks → implement); explains the dev process, not code changes.
