---

description: "Task list template for feature implementation"
---

# Tasks: Keycloak Authentication Endpoints

**Input**: Design documents from `/specs/015-keycloak-auth-endpoints/`

**Prerequisites**: plan.md (required), spec.md (required for user stories), research.md, data-model.md, contracts/

**Tests**: The examples below include test tasks. Tests are OPTIONAL - only include them if explicitly requested in the feature specification.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3)
- Include exact file paths in descriptions

## Path Conventions

- **API project**: `Shelfly.Api/` at repository root
- **Test project**: `Shelfly.Api.Tests/` at repository root
- **Features**: `Shelfly.Api/Features/Auth/` for vertical slice architecture
- Paths shown below follow the plan.md structure

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Project initialization and basic structure

- [X] T001 Create feature directory structure at `Shelfly.Api/Features/Auth/` with subdirectories: Endpoints, Services, Validators, DTOs, Results
- [X] T002 Create test directories in `Shelfly.Api.Tests/`: Integration/, Unit/
- [X] T003 [P] Add Testcontainers.Keycloak package to `Directory.Packages.props` and reference in `Shelfly.Api.Tests/Shelfly.Api.Tests.csproj`

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T004 Implement KeycloakAdminClient service in `Shelfly.Api/Features/Auth/Services/KeycloakAdminClient.cs` with IHttpClientFactory injection for Admin API communication
- [X] T005 [P] Create AuthResult base type in `Shelfly.Api/Features/Auth/Results/AuthResult.cs` implementing Result pattern for success/failure outcomes
- [X] T006 [P] Configure OpenTelemetry instrumentation for authentication endpoints in `Shelfly.Api/Program.cs` with custom span attributes for user ID, operation type, and outcome correlation
- [X] T007 Implement rate limiting service using MongoDB in `Shelfly.Api/Features/Auth/Services/RateLimitService.cs` with configurable thresholds (5 failures/minute, lockout after 10 failures in 15 minutes)
- [X] T008 Create AuthEndpointExtensions registration method in `Shelfly.Api/Extensions/AuthEndpointExtensions.cs` for clean endpoint mapping syntax
- [X] T009 Register authentication services and validators in DI container via `Shelfly.Api/Program.cs` with Polly retry policy for MongoDB resilience

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - New User Registration (Priority: P1) 🎯 MVP

**Goal**: A new user provides an email address and password to create a Shelfly account. The system creates the corresponding Keycloak user and returns confirmation with the assigned user identifier.

**Independent Test**: Can be fully tested by submitting a valid email/password pair and verifying that a new Keycloak account exists with the returned user ID.

### Tests for User Story 1 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T010 [P] [US1] Integration test for registration endpoint in `Shelfly.Api.Tests/Integration/AuthEndpointsTests.cs` covering success (201), conflict (409), and validation failure (400) scenarios
- [ ] T011 [P] [US1] Unit test for RegisterRequestValidator in `Shelfly.Api.Tests/Unit/ValidatorTests.cs` verifying email format and password length rules

### Implementation for User Story 1

- [X] T012 [P] [US1] Create RegisterRequestDto in `Shelfly.Api/Features/Auth/DTOs/RegisterRequestDto.cs` with Email, Password, FirstName, LastName properties
- [X] T013 [P] [US1] Create AuthResponseDto in `Shelfly.Api/Features/Auth/DTOs/AuthResponseDto.cs` with UserId, Email, Status properties for registration confirmation
- [X] T014 [P] [US1] Implement RegisterRequestValidator in `Shelfly.Api/Features/Auth/Validators/RegisterRequestValidator.cs` using FluentValidation with email format and 8-character minimum password rules
- [X] T015 [US1] Implement IAuthService interface in `Shelfly.Api/Features/Auth/Services/IAuthService.cs` with RegisterAsync method signature returning AuthResult<AuthResponseDto>
- [X] T016 [US1] Implement AuthService.RegisterAsync in `Shelfly.Api/Features/Auth/Services/AuthService.cs` delegating to Keycloak Admin API (`POST /admin/realms/{realm}/users`) with 409 conflict handling for duplicate emails
- [X] T017 [US1] Implement RegisterEndpoint in `Shelfly.Api/Features/Auth/Endpoints/RegisterEndpoint.cs` mapping POST `/v1/auth/register` with FluentValidation integration and RFC 7807 Problem Details error responses
- [X] T018 [US1] Add OpenTelemetry span attributes for registration events (user ID, email, outcome) in `Shelfly.Api/Features/Auth/Endpoints/RegisterEndpoint.cs`

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently

---

## Phase 4: User Story 2 - User Login (Priority: P1) 🎯 MVP

**Goal**: An existing user provides their email and password to authenticate against Keycloak. Upon success, the system issues an access token (JWT) and refresh token for subsequent API calls.

**Independent Test**: Can be fully tested by logging in with known credentials and verifying that a valid JWT access token and refresh token are returned, which can then authenticate subsequent requests.

### Tests for User Story 2 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T019 [P] [US2] Integration test for login endpoint in `Shelfly.Api.Tests/Integration/AuthEndpointsTests.cs` covering success (200), invalid credentials (401), and validation failure (400) scenarios
- [ ] T020 [P] [US2] Unit test for LoginRequestValidator in `Shelfly.Api.Tests/Unit/ValidatorTests.cs` verifying email and password presence rules

### Implementation for User Story 2

- [X] T021 [P] [US2] Create LoginRequestDto in `Shelfly.Api/Features/Auth/DTOs/LoginRequestDto.cs` with Email, Password properties
- [X] T022 [P] [US2] Implement LoginRequestValidator in `Shelfly.Api/Features/Auth/Validators/LoginRequestValidator.cs` using FluentValidation with required email and password rules
- [X] T023 [US2] Extend IAuthService interface in `Shelfly.Api/Features/Auth/Services/IAuthService.cs` with LoginAsync method signature returning AuthResult<AuthResponseDto>
- [X] T024 [US2] Implement AuthService.LoginAsync in `Shelfly.Api/Features/Auth/Services/AuthService.cs` delegating to Keycloak token endpoint (`POST {issuer}/protocol/openid-connect/token`) with rate limiting integration and 401 handling for invalid credentials
- [X] T025 [US2] Implement LoginEndpoint in `Shelfly.Api/Features/Auth/Endpoints/LoginEndpoint.cs` mapping POST `/v1/auth/login` with FluentValidation, rate limit headers (X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset), and RFC 7807 Problem Details error responses
- [X] T026 [US2] Add OpenTelemetry span attributes for login events (user ID, email, outcome, rate limit status) in `Shelfly.Api/Features/Auth/Endpoints/LoginEndpoint.cs`

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently

---

## Phase 5: User Story 3 - Token Refresh (Priority: P2)

**Goal**: An authenticated user whose access token has expired uses their refresh token to obtain a new access token without re-entering credentials.

**Independent Test**: Can be fully tested by obtaining a login response, waiting (or simulating) token expiration, and using the refresh token to acquire a fresh access token.

### Tests for User Story 3 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T027 [P] [US3] Integration test for refresh endpoint in `Shelfly.Api.Tests/Integration/AuthEndpointsTests.cs` covering success (200), expired token (401), and validation failure (400) scenarios
- [ ] T028 [P] [US3] Unit test for RefreshRequestValidator in `Shelfly.Api.Tests/Unit/ValidatorTests.cs` verifying refresh token presence rules

### Implementation for User Story 3

- [X] T029 [P] [US3] Create RefreshRequestDto in `Shelfly.Api/Features/Auth/DTOs/RefreshRequestDto.cs` with RefreshToken property
- [X] T030 [P] [US3] Implement RefreshRequestValidator in `Shelfly.Api/Features/Auth/Validators/RefreshRequestValidator.cs` using FluentValidation with required refresh token rule
- [X] T031 [US3] Extend IAuthService interface in `Shelfly.Api/Features/Auth/Services/IAuthService.cs` with RefreshAsync method signature returning AuthResult<AuthResponseDto>
- [X] T032 [US3] Implement AuthService.RefreshAsync in `Shelfly.Api/Features/Auth/Services/AuthService.cs` delegating to Keycloak token endpoint (`POST {issuer}/protocol/openid-connect/token` with grant_type=refresh_token) and 401 handling for expired tokens
- [X] T033 [US3] Implement RefreshEndpoint in `Shelfly.Api/Features/Auth/Endpoints/RefreshEndpoint.cs` mapping POST `/v1/auth/refresh` with FluentValidation and RFC 7807 Problem Details error responses
- [X] T034 [US3] Add OpenTelemetry span attributes for refresh events (user ID from token, outcome) in `Shelfly.Api/Features/Auth/Endpoints/RefreshEndpoint.cs`

**Checkpoint**: All user stories should now be independently functional

---

## Phase 6: User Story 4 - Password Reset (Priority: P2)

**Goal**: A user who forgot their password submits their email address to initiate a reset. The system sends a Keycloak-generated reset link and, upon completion, allows the user to log in with the new credentials.

**Independent Test**: Can be fully tested by submitting a registered email, receiving a reset confirmation, and verifying that subsequent login succeeds with the new password.

### Tests for User Story 4 ⚠️

> **NOTE: Write these tests FIRST, ensure they FAIL before implementation**

- [ ] T035 [P] [US4] Integration test for password reset endpoint in `Shelfly.Api.Tests/Integration/AuthEndpointsTests.cs` covering success (200), unregistered email (404 or generic 200), and validation failure (400) scenarios
- [ ] T036 [P] [US4] Unit test for ResetPasswordRequestValidator in `Shelfly.Api.Tests/Unit/ValidatorTests.cs` verifying email format rules

### Implementation for User Story 4

- [X] T037 [P] [US4] Create ResetPasswordRequestDto in `Shelfly.Api/Features/Auth/DTOs/ResetPasswordRequestDto.cs` with Email property
- [X] T038 [P] [US4] Implement ResetPasswordRequestValidator in `Shelfly.Api/Features/Auth/Validators/ResetPasswordRequestValidator.cs` using FluentValidation with email format rule
- [X] T039 [US4] Extend IAuthService interface in `Shelfly.Api/Features/Auth/Services/IAuthService.cs` with ResetPasswordAsync method signature returning AuthResult<string> for confirmation message
- [X] T040 [US4] Implement AuthService.ResetPasswordAsync in `Shelfly.Api/Features/Auth/Services/AuthService.cs` delegating to Keycloak Admin API (`POST /admin/realms/{realm}/users/{id}/execute-actions-email` with ACTION: UPDATE_PASSWORD) and 404 handling for unregistered emails
- [X] T041 [US4] Implement ResetPasswordEndpoint in `Shelfly.Api/Features/Auth/Endpoints/ResetPasswordEndpoint.cs` mapping POST `/v1/auth/reset-password` with FluentValidation and RFC 7807 Problem Details error responses
- [X] T042 [US4] Add OpenTelemetry span attributes for password reset events (email, outcome) in `Shelfly.Api/Features/Auth/Endpoints/ResetPasswordEndpoint.cs`

**Checkpoint**: All user stories should now be independently functional

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [X] T043 [P] Update AuthEndpointExtensions in `Shelfly.Api/Extensions/AuthEndpointExtensions.cs` to register all authentication endpoints with proper route grouping
- [X] T044 Add comprehensive error handling middleware for Keycloak connectivity failures (502 Bad Gateway) in `Shelfly.Api/Program.cs`
- [ ] T045 [P] Run quickstart.md validation scenarios covering registration, login, token refresh, password reset, and rate limiting flows _(deferred - requires infrastructure setup)_
- [X] T046 Code cleanup and refactoring across all authentication components
- [ ] T047 Security hardening: verify JWT audience validation, token expiration handling, and sensitive data masking in logs _(deferred - requires manual verification)_

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Stories (Phase 3+)**: All depend on Foundational phase completion
  - User stories can then proceed in parallel (if staffed)
  - Or sequentially in priority order (P1 → P2)
- **Polish (Final Phase)**: Depends on all desired user stories being complete

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - No dependencies on other stories
- **User Story 2 (P1)**: Can start after Foundational (Phase 2) - Shares foundational services with US1 but independently testable
- **User Story 3 (P2)**: Can start after Foundational (Phase 2) - Depends on login flow for token acquisition but independently testable
- **User Story 4 (P2)**: Can start after Foundational (Phase 2) - May integrate with US1/US2 but should be independently testable

### Within Each User Story

- Tests (if included) MUST be written and FAIL before implementation
- DTOs before validators
- Validators before services
- Services before endpoints
- Core implementation before integration
- Story complete before moving to next priority

### Parallel Opportunities

- All Setup tasks marked [P] can run in parallel
- All Foundational tasks marked [P] can run in parallel (within Phase 2)
- Once Foundational phase completes, all user stories can start in parallel (if team capacity allows)
- All tests for a user story marked [P] can run in parallel
- DTOs and validators within a story marked [P] can run in parallel
- Different user stories can be worked on in parallel by different team members

---

## Parallel Example: User Story 1

```bash
# Launch all tests for User Story 1 together:
Task: "Integration test for registration endpoint in Shelfly.Api.Tests/Integration/AuthEndpointsTests.cs"
Task: "Unit test for RegisterRequestValidator in Shelfly.Api.Tests/Unit/ValidatorTests.cs"

# Launch all DTOs and validators for User Story 1 together:
Task: "Create RegisterRequestDto in Shelfly.Api/Features/Auth/DTOs/RegisterRequestDto.cs"
Task: "Create AuthResponseDto in Shelfly.Api/Features/Auth/DTOs/AuthResponseDto.cs"
Task: "Implement RegisterRequestValidator in Shelfly.Api/Features/Auth/Validators/RegisterRequestValidator.cs"
```

---

## Implementation Strategy

### MVP First (User Story 1 Only)

1. Complete Phase 1: Setup
2. Complete Phase 2: Foundational (CRITICAL - blocks all stories)
3. Complete Phase 3: User Story 1
4. **STOP and VALIDATE**: Test User Story 1 independently
5. Deploy/demo if ready

### Incremental Delivery

1. Complete Setup + Foundational → Foundation ready
2. Add User Story 1 → Test independently → Deploy/Demo (MVP!)
3. Add User Story 2 → Test independently → Deploy/Demo
4. Add User Story 3 → Test independently → Deploy/Demo
5. Add User Story 4 → Test independently → Deploy/Demo
6. Each story adds value without breaking previous stories

### Parallel Team Strategy

With multiple developers:

1. Team completes Setup + Foundational together
2. Once Foundational is done:
   - Developer A: User Story 1 (Registration)
   - Developer B: User Story 2 (Login)
   - Developer C: User Story 3 (Token Refresh)
   - Developer D: User Story 4 (Password Reset)
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
