# Research: Local Library Management

**Date**: 2026-08-18 | **Feature**: 002-local-library

## Decisions

### Decision 1: Local Storage Technology

**Decision**: Use SQLite with EF Core for local data persistence in the MAUI client.

**Rationale**: 
- `Microsoft.EntityFrameworkCore.Sqlite` (v10.0.11) already available in `Directory.Packages.props`
- Spec clarified that local SQLite database is the storage technology
- Provides relational query capabilities needed for search, sorting, and filtering
- Cross-platform support across Android, iOS, Windows, MacOS

**Alternatives considered**:
- Realm: Not yet approved; would require new NuGet package addition
- JSON file storage: Lacks query/filtering capabilities for search and sort operations
- SQLite-net-pcl: Less feature-rich than EF Core; no migration support

### Decision 2: MVVM Pattern with Shell Navigation

**Decision**: Follow constitution Principle III — use MVVM pattern with Shell navigation, scoped DI registration via `AddScopedWithShellRoute<TPage, TViewModel>("route")`.

**Rationale**:
- Constitution mandates this pattern for the MAUI client
- Enables constructor injection on both pages and view models
- ViewModels recreated per navigation (prevents stale state)
- XAML source generation (`MauiXamlInflator=SourceGen`) catches binding errors at compile time

**Alternatives considered**:
- Manual DI registration: More boilerplate, less type-safe route mapping
- Prism framework: Requires additional NuGet package; not yet approved

### Decision 3: Localization Approach

**Decision**: Use .NET MAUI native resource files (`AppResources.resx`) for German and English localization. Language switching handled natively by MAUI runtime.

**Rationale**:
- Spec clarified that language switching is handled natively by .NET MAUI without data loss
- No additional dependencies required (built into MAUI framework)
- Supports runtime language switching via `CurrentUICulture` changes
- Resource files provide compile-time validation of string keys

**Alternatives considered**:
- FluentValidation localization: Requires separate resource management; adds complexity
- JSON-based localization: Runtime-only, no compile-time validation

### Decision 4: Inline Validation Error Display

**Decision**: Use .NET MAUI equivalent of Android supporting text for inline validation errors on each field. Each error displayed independently on the respective text field.

**Rationale**:
- Spec clarified that each validation error is displayed inline on the respective text field
- Provides immediate, field-specific feedback to users
- Consistent with platform-native UX patterns (Android supporting text)
- Multiple simultaneous errors all visible at once

**Alternatives considered**:
- Toast/alert dialog: Shows one error at a time; requires user interaction per error
- Summary validation list: Requires scrolling; less direct association between field and error

### Decision 5: Swipe-to-Delete Gesture Implementation

**Decision**: Use .NET MAUI `SwipeView` for mobile platforms; platform-native drag/swipe equivalent on desktop (Windows/MacOS).

**Rationale**:
- Spec clarified that desktop platforms use platform-native drag/swipe equivalent
- `SwipeView` provides native swipe gesture support on Android/iOS
- Desktop fallback ensures consistent UX across all target platforms
- Soft delete behavior (`DeletedAt` timestamp) per constitution Principle V

**Alternatives considered**:
- Context menu: Less discoverable; requires right-click/long press
- Dedicated delete button only: Requires extra navigation step from list view

### Decision 6: Bookmark List Ordering for Overlapping Pages

**Decision**: Display overlapping bookmarks as separate entries; range-based bookmarks appear first, followed by single-page bookmarks when referencing the same page.

**Rationale**:
- Spec clarified ordering rules explicitly
- Range-first ordering provides broader context before specific page notes
- Separate entries maintain bookmark independence (users can edit/delete individually)

**Alternatives considered**:
- Alphabetical by note: Less intuitive for page-based navigation
- Chronological by creation date: Does not prioritize range vs single-page distinction

### Decision 7: Library Data Export Format

**Decision**: Use JSON format for library data export (backup only — no import capability in this feature).

**Rationale**:
- Spec clarified that export-only is sufficient for backup purposes
- JSON provides human-readable, portable format compatible with all platforms
- .NET System.Text.Json enables serialization without additional dependencies
- Simpler than CSV for nested structures (book → bookmarks relationship)

**Alternatives considered**:
- CSV: Flattens hierarchical data; loses bookmark-to-book relationships
- XML: Verbose; less commonly used for modern backup formats
- SQLite file copy: Platform-dependent file paths; less portable across devices

### Decision 8: Library Capacity Management

**Decision**: No explicit book count limit — rely on device storage capacity (practical limit ~10,000 books).

**Rationale**:
- Spec clarified that no upper bound is needed for personal libraries
- SQLite handles typical library sizes without performance degradation
- Avoids arbitrary constraints and simplifies implementation
- Device storage naturally bounds the practical maximum

**Alternatives considered**:
- Hard limit of 1,000 books: Too restrictive for avid readers
- Soft limit with warning at 5,000: Adds UI complexity for edge case
- Pagination-only approach: Defers capacity concern but requires additional UX design
