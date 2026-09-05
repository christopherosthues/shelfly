# Shelfly.App — Agent Notes

## Boundaries

- The MAUI client only talks to the API, never Keycloak directly.

## Build Quirks

- **Conditional MAUI targets**: vary by host OS (`net10.0-android` always; iOS/MacCatalyst on non-Linux; Windows only on Windows). Build with `dotnet build Shelfly.App/Shelfly.App.csproj` so MSBuild resolves conditionally.
- **XAML source generation**: `<MauiXamlInflator>SourceGen</MauiXamlInflator>` generates C# from XAML at compile time.
