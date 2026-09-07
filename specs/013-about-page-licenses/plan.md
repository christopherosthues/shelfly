# Implementation Plan: About Page Licenses

**Branch**: `[013-about-page-licenses]` | **Date**: 2026-09-07 | **Spec**: [spec.md](./spec.md)

**Input**: Feature specification from `/specs/013-about-page-licenses/spec.md`

## Summary

Replace the hardcoded alert dialog in the About page with a proper scrollable CollectionView displaying all third-party NuGet dependencies. The dependency data is generated at build time using `nuget-license` CLI and bundled as a MauiAsset resource, then parsed at runtime to populate the list. Each entry shows package name, version, author(s), and license type; tapping a license opens its URL externally.

## Technical Context

**Language/Version**: C# / .NET 10 (MAUI)

**Primary Dependencies**: System.Text.Json (built-in), CommunityToolkit.Mvvm (8.4.2), Microsoft.Maui.Controls (10.0.100)

**Storage**: Bundled JSON file as MauiAsset resource (no database changes)

**Testing**: TUnit unit tests + Shouldly assertions; existing test project `Shelfly.App.Tests`

**Target Platform**: Android (primary); iOS/MacCatalyst/Windows (conditional)

**Project Type**: .NET MAUI mobile application

**Performance Goals**: Dependency list renders within 3 seconds of page load

**Constraints**: JSON file generated only during Release build; no runtime network calls for data loading

**Scale/Scope**: Single page modification (AboutPage); ~100-200 dependency entries expected

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

No constitution file exists at `.specify/memory/constitution.md` — gates deferred to project-level AGENTS.md conventions:
- Nullable reference types enabled ✓
- Primary constructors preferred ✓
- Collection expressions (`[]`) over `new List<T>()` ✓
- MVVM Toolkit patterns ([ObservableProperty], [RelayCommand]) ✓

## Project Structure

### Documentation (this feature)

```text
specs/013-about-page-licenses/
├── plan.md              # This file
├── research.md          # Phase 0 output
├── data-model.md        # Phase 1 output
├── quickstart.md        # Phase 1 output
├── contracts/           # Phase 1 output (if applicable)
└── tasks.md             # Phase 2 output (/speckit.tasks command)
```

### Source Code (repository root)

```text
Shelfly.App/
├── Features/About/
│   ├── AboutPage.xaml              # Modified: CollectionView replaces button
│   ├── AboutPage.xaml.cs           # Unchanged
│   └── AboutViewModel.cs           # Modified: loads JSON, populates list
├── Models/                         # New directory for local data models
│   └── DependencyPackage.cs        # Record for parsed dependency entry
├── Services/                       # New service for license data loading
│   └── LicenseDataService.cs       # Reads and parses bundled JSON
├── Resources/Raw/
│   └── licenses-release.json       # Bundled at build time by nuget-license
└── Shelfly.App.csproj              # Modified: pre-build target + MauiAsset entry
```

**Structure Decision**: All changes confined to `Shelfly.App`. The feature adds a local data model (`DependencyPackage` record), a service for loading the bundled JSON, and modifies the existing About page UI. No API or database changes required because the data is static at build time.

## Complexity Tracking

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| New Models directory | DependencyPackage record needs a home | Could inline in ViewModel but service pattern requires separate type |
