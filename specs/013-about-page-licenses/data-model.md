# Data Model: About Page Licenses

**Date**: 2026-09-07

## DependencyPackage Record

Represents a single third-party NuGet package entry parsed from the bundled JSON file.

### Fields

| Field | Type | Source (JSON) | Display Purpose |
|-------|------|---------------|-----------------|
| PackageId | string | `PackageId` | Primary identifier; displayed as package name |
| PackageVersion | string | `PackageVersion` | Version number; displayed alongside name |
| Authors | string? | `Authors` | Author(s); displayed per entry |
| License | string? | `License` | License type; clickable to open license URL |
| LicenseUrl | Uri? | `LicenseUrl` | Full license text URL; opened externally on tap |

### Validation Rules

- PackageId is required and non-empty (primary key from JSON)
- PackageVersion defaults to "Unknown" if null/empty in source data
- Authors defaults to "Unknown" if null/empty in source data
- License defaults to "Unspecified" if null/empty in source data
- LicenseUrl may be null; fallback behavior defined in service layer

### Relationships

No relationships — this is a flat, standalone record with no foreign keys or references.

## JSON Source Schema (nuget-license output)

The bundled `licenses-release.json` file contains an array of objects with these fields:

```json
[
  {
    "PackageId": "string",
    "PackageVersion": "string",
    "PackageProjectUrl": "string?",
    "Copyright": "string?",
    "Authors": "string?",
    "Description": "string?",
    "License": "string?",
    "LicenseUrl": "string?",
    "LicenseInformationOrigin": "int"
  }
]
```

Only `PackageId`, `PackageVersion`, `Authors`, `License`, and `LicenseUrl` are mapped to the DependencyPackage record. Remaining fields are available for future extensions but not currently used.

## Error State Model

When JSON loading fails, the ViewModel displays a single error entry:

| Field | Value |
|-------|-------|
| PackageId | "Error" |
| PackageVersion | "" |
| Authors | "Failed to load dependency data" |
| License | "" |
| LicenseUrl | null |
