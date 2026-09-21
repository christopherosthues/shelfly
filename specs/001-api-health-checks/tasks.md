---

description: "Task list for API health checks implementation"

---

# Tasks: API Health Checks

**Input**: Design documents from `/specs/001-api-health-checks/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/health-endpoints.md, quickstart.md

**Tests**: Included — constitution requires TUnit unit tests and Testcontainers integration tests.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

Paths use the project structure from `plan.md`:

```text
Shelfly.Api/Features/HealthChecks/    # Feature implementation
Shelfly.Api.Tests/Features/HealthChecks/  # Tests
```

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and package configuration

- [X] T001 Add `Microsoft.Extensions.Diagnostics.HealthChecks` version 10.0.12 to `Directory.Packages.props`
- [X] T002 Add package reference for `Microsoft.Extensions.Diagnostics.HealthChecks` to `Shelfly.Api/Shelfly.Api.csproj`
- [X] T003 Create feature directory structure: `Shelfly.Api/Features/HealthChecks/Checks/`, `Endpoints/`, `DTOs/`, `Services/`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core health check infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Register health check services in `Shelfly.Api/Program.cs` using `builder.Services.AddHealthChecks()`
- [X] T005 [P] Create HealthCheckResponseDto in `Shelfly.Api/Features/HealthChecks/DTOs/HealthCheckResponseDto.cs`
- [X] T006 [P] Create DependencyStatusDto in `Shelfly.Api/Features/HealthChecks/DTOs/DependencyStatusDto.cs`
- [X] T007 Implement custom ResponseWriter delegate for structured JSON health responses in `Shelfly.Api/Program.cs`
- [X] T008 Configure HealthCheckOptions with ResultStatusCodes mapping (Healthy→200, Unhealthy→503) in `Shelfly.Api/Program.cs`

**Checkpoint**: Foundation ready — user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Basic Liveness Probe (Priority: P1) 🎯 MVP

**Goal**: Operations teams and orchestrators can verify the API process is running by hitting a simple endpoint that returns an immediate success response.

**Independent Test**: Send HTTP GET to `/v1/health/live` and receive HTTP 200 OK with `"Status": "Healthy"` in response body, latency <200ms.

### Tests for User Story 1 ⚠️

> **NOTE: Write these tests FIRST, before implementation**

- [X] T009 [P] [US1] Unit test for liveness health check logic in `Shelfly.Api.Tests/Features/HealthChecks/Unit/LivenessHealthCheckTests.cs`
- [X] T010 [P] [US1] Contract test for `/v1/health/live` endpoint response format in `Shelfly.Api.Tests/Features/HealthChecks/Integration/HealthEndpointIntegrationTests.cs`

### Implementation for User Story 1

- [X] T011 [P] [US1] Implement LivenessHealthCheck class in `Shelfly.Api/Features/HealthChecks/Checks/LivenessHealthCheck.cs`
- [X] T012 [US1] Register liveness health check with tag "live" in `builder.Services.AddHealthChecks()` at `Shelfly.Api/Program.cs`
- [X] T013 [US1] Create HealthEndpointExtensions with MapLiveHealthChecks method in `Shelfly.Api/Features/HealthChecks/Endpoints/HealthEndpointExtensions.cs`
- [X] T014 [US1] Map `/v1/health/live` endpoint with predicate filter for "live" tag in `Shelfly.Api/Program.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - Dependency Readiness Probe (Priority: P2)

**Goal**: Operations teams can verify that all critical backend dependencies (PostgreSQL, MongoDB, Keycloak) are reachable and functional.

**Independent Test**: Send HTTP GET to `/v1/health/ready` with all dependencies available → HTTP 200; stop one dependency → HTTP 503 with failure category in response body.

### Tests for User Story 2 ⚠️

> **NOTE: Write these tests FIRST, before implementation**

- [X] T015 [P] [US2] Unit test for PostgreSQL health check logic in `Shelfly.Api.Tests/Features/HealthChecks/Unit/PostgreSQLHealthCheckTests.cs`
- [X] T016 [P] [US2] Unit test for MongoDB health check logic in `Shelfly.Api.Tests/Features/HealthChecks/Unit/MongoDBHealthCheckTests.cs`
- [X] T017 [P] [US2] Unit test for Keycloak health check logic in `Shelfly.Api.Tests/Features/HealthChecks/Unit/KeycloakHealthCheckTests.cs`
- [X] T018 [P] [US2] Integration test for `/v1/health/ready` with Testcontainers (PostgreSQL + MongoDB) in `Shelfly.Api.Tests/Features/HealthChecks/Integration/ReadinessEndpointIntegrationTests.cs`

### Implementation for User Story 2

- [X] T019 [P] [US2] Implement PostgreSQLHealthCheck using Npgsql connection + query in `Shelfly.Api/Features/HealthChecks/Checks/PostgreSQLHealthCheck.cs`
- [X] T020 [P] [US2] Implement MongoDBHealthCheck using ping command on admin database in `Shelfly.Api/Features/HealthChecks/Checks/MongoDBHealthCheck.cs`
- [X] T021 [P] [US2] Implement KeycloakHealthCheck using HTTP GET to realm endpoint in `Shelfly.Api/Features/HealthChecks/Checks/KeycloakHealthCheck.cs`
- [X] T022 [US2] Register readiness health checks with tags and 3-second timeout in `builder.Services.AddHealthChecks()` at `Shelfly.Api/Program.cs`
- [X] T023 [US2] Add MapReadyHealthChecks method to HealthEndpointExtensions in `Shelfly.Api/Features/HealthChecks/Endpoints/HealthEndpointExtensions.cs`
- [X] T024 [US2] Map `/v1/health/ready` endpoint with predicate filter for "ready" tag in `Shelfly.Api/Program.cs`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Health Status Reporting (Priority: P3)

**Goal**: The health check response includes structured data describing each dependency's status for monitoring dashboards and alerting systems.

**Independent Test**: Parse JSON response from `/v1/health/ready` and validate that it contains named dependency entries with pass/fail indicators and failure categories.

### Tests for User Story 3 ⚠️

> **NOTE: Write these tests FIRST, before implementation**

- [X] T025 [P] [US3] Unit test for response writer JSON structure in `Shelfly.Api.Tests/Features/HealthChecks/Unit/HealthCheckResponseWriterTests.cs`
- [X] T026 [P] [US3] Contract test verifying dependency status fields in `/v1/health/ready` response in `Shelfly.Api.Tests/Features/HealthChecks/Integration/ReadinessEndpointIntegrationTests.cs`

### Implementation for User Story 3

- [X] T027 [P] [US3] Implement HealthCheckResultService for result aggregation and failure categorization in `Shelfly.Api/Features/HealthChecks/Services/HealthCheckResultService.cs`
- [X] T028 [US3] Update ResponseWriter to serialize HealthCheckResponseDto with dependency details using System.Text.Json in `Shelfly.Api/Program.cs`
- [X] T029 [US3] Implement failure category mapping logic (timeout, connection refused, other) in `Shelfly.Api/Features/HealthChecks/Services/HealthCheckResultService.cs`

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T030 [P] Add OpenTelemetry instrumentation for health check spans in `Shelfly.Api/Program.cs`
- [ ] T031 [P] Add structured logging for health check results (dependency name, status, duration) in all health check implementations
- [ ] T032 Run quickstart.md validation scenarios against running API
- [ ] T033 Verify Docker HEALTHCHECK directive compatibility with `/v1/health/live` endpoint

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies — can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion — BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2 → P3)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) — No dependencies on other stories
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) — Uses same endpoint extension as US1 but independently testable
- **User Story 3 (P3)**: Depends on US2 completion — enhances readiness response format

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- Models before services
- Services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- Models within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Unit test for liveness health check logic in Shelfly.Api.Tests/Features/HealthChecks/Unit/LivenessHealthCheckTests.cs"
Task: "Contract test for /v1/health/live endpoint response format in Shelfly.Api.Tests/Features/HealthChecks/Integration/HealthEndpointIntegrationTests.cs"

# Launch all models for User Story 1 together:
Task: "Implement LivenessHealthCheck class in Shelfly.Api/Features/HealthChecks/Checks/LivenessHealthCheck.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL — blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently via `curl http://localhost:5000/v1/health/live`
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (liveness)
   - Developer B: User Story 2 (readiness checks)
   - Developer C: User Story 3 (structured reporting)
3. Stories complete and integrate independently

---

## Notes

- [P] tasks = different files, no dependencies
- [Story] label maps task to specific user story for traceability
- Each user story should be independently completable and testable
- Verify tests fail before implementing
- Commit after each task or logical group
- Stop at any checkpoint to validate story independently
- Avoid: vague tasks, same file conflicts, cross-story dependencies that break independence
