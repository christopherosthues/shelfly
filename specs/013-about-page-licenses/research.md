# Research: About Page Licenses

**Date**: 2026-09-07

## Decision: JSON File Bundling Approach

**Decision**: Use `<None>` item with `CopyToOutputDirectory="Always"` to bundle the generated JSON file, accessible at runtime via application output directory path.

**Rationale**: The nuget-license tool generates a standalone JSON file. Using `<None CopyToOutputDirectory="Always">` ensures the file is copied to the app's output directory on every build, making it accessible via `AppDomain.CurrentDomain.BaseDirectory`. This avoids embedding as a resource (which requires stream reading) and keeps the file easily inspectable during debugging.

**Alternatives considered**:
- `<EmbeddedResource>`: Requires reading from assembly manifest resources via streams — more complex for JSON parsing
- `<MauiAsset>`: Designed for media assets; overkill for a data file
- Build server-side target (`<Target BeforeTargets="Build">`): Runs `nuget-license` automatically during Release builds

## Decision: Pre-Build Target for nuget-license

**Decision**: Add a MSBuild target that runs `nuget-license -i Shelfly.App.csproj -o JsonPretty --include-transitive -fo licenses-release.json` before the build, conditioned on Release configuration.

**Rationale**: The spec requires JSON generation at build time. A pre-build target ensures the file is always fresh when building in Release mode. The tool is installed globally, so no NuGet package reference needed.

**Alternatives considered**:
- Manual script execution: Requires developer discipline; error-prone for CI/CD
- Post-publish event: Too late — file not available during app runtime testing
- CI-only generation: Works for production but complicates local development

## Decision: Data Model Shape

**Decision**: Create a simple record type `DependencyPackage` with properties matching the nuget-license JSON schema: PackageId, PackageVersion, Authors, License, LicenseUrl.

**Rationale**: The JSON structure from nuget-license is well-defined and stable. A record provides immutability and concise syntax aligned with project conventions (primary constructors, C# 12 features). Only the fields needed for display are mapped; extra fields like Description and Copyright are available but not required by the spec.

**Alternatives considered**:
- Anonymous types: Less testable and harder to reference across layers
- Full JSON property mapping: Overkill — only name, version, authors, license, and URL are displayed

## Decision: UI Integration Approach

**Decision**: Replace the "View Licenses" button with an inline CollectionView section in AboutPage.xaml, displaying all dependencies as a scrollable list within the existing page.

**Rationale**: The spec requires viewing the complete dependency list on the About page. An inline CollectionView eliminates an extra navigation step and keeps all app information on one screen. The existing ScrollView already wraps the content, so adding a CollectionView section fits naturally.

**Alternatives considered**:
- Separate LicensesPage: Adds navigation complexity; the spec says "on the About page"
- Keep button but show expanded list inline: Requires toggle state management — unnecessary complexity for a single-page feature

## Decision: License URL Navigation

**Decision**: Use `Launcher.OpenAsync()` from `CommunityToolkit.Maui` to open license URLs in an external browser when tapped.

**Rationale**: CommunityToolkit.Maui is already referenced and provides cross-platform web navigation. The existing AboutViewModel uses `Shell.Current.DisplayAlertAsync()`, showing the project already leverages MAUI shell utilities. Launcher.OpenAsync handles platform differences (Android intent, iOS UIApplication, Windows Process.Start) automatically.

**Alternatives considered**:
- WebView inline: Requires additional page layout; heavier resource usage
- Shell navigation to web URL: Works but Launcher is more idiomatic for external links

## Decision: Error Handling for Missing JSON

**Decision**: Display a user-friendly error message in the CollectionView when the JSON file is missing or malformed, using a fallback ObservableCollection with a single error entry.

**Rationale**: FR-006 requires graceful handling. Showing an error item within the list maintains UI consistency and informs the user without crashing. The ViewModel's LoadAsync method can catch exceptions and set a visible error state.

**Alternatives considered**:
- Alert dialog on load: Interrupts user flow; less discoverable
- Empty list with toast message: May be missed by users
