# Feature Specification: About Page Licenses

**Feature Branch**: `[013-about-page-licenses]`

**Created**: 2026-09-07

**Status**: Draft

**Input**: User description: "Another agent imagined the plan in about-page-licenses-plan.txt. But I think it would be better to use the nuget-license tool from https://github.com/sensslen/nuget-license which I already installed globally. The Shelfly.App project has to be restored first with "dotnet restore -c Release" and then "nuget-license -i Shelfly.App.csproj -o JsonPretty --include-transitive -fo licenses-release.json" to generate the JSON file containing all information we need for the About page to display all dependencies with version and license + license link."

## Clarifications

### Session 2026-09-07

- Q: Is the JSON dependency file generated at build time and bundled with the app? → A: Yes, generated at build time and bundled as an embedded resource
- Q: Is a search/filter feature required for the dependency list? → A: No search feature required
- Q: Should transitive dependencies be visually distinguished from direct dependencies in the list? → A: Show all dependencies in a single flat list without distinction

## User Scenarios & Testing *(mandatory)*

### User Story 1 - View Third-Party Dependencies List (Priority: P1)

The user opens the About page and sees a scrollable list of all third-party NuGet packages used by the application, including package name, version, author(s), and license type.

**Why this priority**: This is the core value of the feature — transparency about open-source dependencies and their licenses, which is essential for compliance and user trust.

**Independent Test**: Can be fully tested by navigating to the About page and verifying that a list of dependencies appears with correct package names, versions, and license information.

**Acceptance Scenarios**:

1. **Given** the user is on the About page, **When** they scroll through the dependency list, **Then** each entry displays the package name, version number, author(s), and license type
2. **Given** a package has multiple authors, **When** viewing its entry, **Then** all authors are displayed
3. **Given** a package uses a compound license (e.g., "MIT AND Apache-2.0"), **When** viewing its entry, **Then** the full license string is displayed

---

### User Story 2 - Navigate to License Details (Priority: P2)

The user taps on a dependency's license type to open the corresponding license text in an external browser or viewer.

**Why this priority**: Allows users to read the full license terms, providing deeper transparency and compliance verification.

**Independent Test**: Can be tested by tapping any license link and confirming it opens the correct license page in a browser.

**Acceptance Scenarios**:

1. **Given** a dependency entry displays a license type, **When** the user taps the license, **Then** the corresponding license URL opens in an external viewer
2. **Given** a package has no license URL, **When** the user taps the license, **Then** a fallback message or standard license page is shown

---

### Edge Cases

- What happens when the generated JSON file is missing or corrupted at runtime?
- How does the system handle packages with unusually long names or descriptions?
- What if a package has no license information (empty License field)?
- Transitive dependencies are displayed identically to direct dependencies in a single flat list

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST generate a JSON file containing all NuGet package dependency information (including transitive dependencies) for the Shelfly.App project during build time
- **FR-002**: System MUST persist the generated JSON file as an embedded resource or bundled asset accessible at runtime
- **FR-003**: Users MUST be able to view a complete list of third-party dependencies on the About page
- **FR-004**: Each dependency entry MUST display package name, version, author(s), and license type
- **FR-005**: System MUST provide clickable navigation from each dependency's license to its full license text via URL
- **FR-006**: System MUST gracefully handle missing or malformed JSON data with a user-friendly error message

### Key Entities

- **Dependency Package**: Represents a third-party NuGet library used by the application; contains package ID, version, project URL, copyright notice, author(s), description, license type, and license URL
- **About Page**: The application page where users can view app information including the dependency list

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can view the complete list of third-party dependencies within 3 seconds of opening the About page
- **SC-002**: 100% of bundled dependencies are displayed with accurate version and license information
- **SC-003**: Users can navigate to a dependency's full license text with a single tap
- **SC-004**: The dependency list remains current when packages are added, removed, or updated (verified at next build)

## Assumptions

- The JSON dependency file is generated at build time and bundled with the application as an embedded resource
- Users have stable internet connectivity when tapping license URLs to view full license text externally
- All NuGet packages included in the Shelfly.App project have valid license information in their metadata
