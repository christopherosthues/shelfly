# Quickstart Validation Guide: About Page Licenses

**Date**: 2026-09-07

## Prerequisites

1. `nuget-license` CLI tool installed globally (`dotnet tool install -g nuget-license`)
2. .NET 10 SDK with MAUI workloads installed
3. Restore completed: `dotnet restore -c Release` (generates `licenses-release.json`)

## Setup Commands

```bash
# From repository root
dotnet restore Shelfly.App/Shelfly.App.csproj -c Release
dotnet build Shelfly.App/Shelfly.App.csproj -c Release
```

The pre-build target in the .csproj will automatically run nuget-license during the Release build, generating `Shelfly.App/Resources/Raw/licenses-release.json`.

## Validation Scenarios

### Scenario 1: Dependency List Displays Correctly

**Run command**:
```bash
dotnet run --project Shelfly.App/Shelfly.App.csproj -c Release
```

**Expected outcome**:
- Application launches and navigates to the About page (via Shell flyout footer)
- A scrollable list of dependencies is visible below the app information section
- Each entry shows package name, version, author(s), and license type
- The list includes both direct and transitive dependencies in a single flat view

### Scenario 2: License URL Navigation Works

**Run command**: Same as above (launch app)

**Expected outcome**:
- Tapping any license type text opens an external browser
- The browser navigates to the correct license URL from the JSON data
- On Android, this opens the default browser via intent
- On Windows/Desktop, this opens the system default browser

### Scenario 3: Missing JSON File Graceful Handling

**Run command**:
```bash
# Temporarily rename the generated file
mv Shelfly.App/Resources/Raw/licenses-release.json Shelfly.App/Resources/Raw/licenses-release.json.bak
dotnet run --project Shelfly.App/Shelfly.App.csproj -c Release
```

**Expected outcome**:
- Application launches without crashing
- The dependency list section shows an error message indicating the data could not be loaded
- No null reference exceptions in the ViewModel

### Scenario 4: Build Time JSON Generation

**Run command**:
```bash
dotnet build Shelfly.App/Shelfly.App.csproj -c Release
```

**Expected outcome**:
- `licenses-release.json` is generated at `Shelfly.App/Resources/Raw/licenses-release.json`
- The file contains valid JSON with dependency entries from all NuGet packages (including transitive)
- Build succeeds without errors even if the previous JSON file was missing or outdated

## References

- **Data model**: See [data-model.md](./data-model.md) for DependencyPackage record structure
- **Research decisions**: See [research.md](./research.md) for bundling and UI approach rationale
