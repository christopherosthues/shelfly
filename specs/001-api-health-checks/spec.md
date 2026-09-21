# Feature Specification: API Health Checks

**Feature Branch**: `[001-api-health-checks]`

**Created**: 2026-09-21

**Status**: Draft

**Input**: User description: "Add health checks for the API"

## Clarifications

### Session 2026-09-21

- Q: How many seconds should elapse before a single dependency check is marked as unhealthy during readiness probes? → A: 3 seconds per dependency
- Q: Should the readiness endpoint require ALL dependencies healthy for HTTP 200, or tolerate partial degradation? → A: Strict — ALL dependencies must be healthy for HTTP 200; any failure → HTTP 503
- Q: Should health check endpoints include detailed error information in the response body when a dependency fails? → A: Categorized summary — dependency name + failure category (no raw errors)

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Liveness Probe (Priority: P1)

Operations teams and orchestrators can verify the API process is running by hitting a simple endpoint that returns an immediate success response.

**Why this priority**: Foundational capability — without liveness verification, deployment pipelines and container orchestration lack basic confidence the service is alive.

**Independent Test**: Can be fully tested by sending an HTTP GET to the health endpoint and receiving a 200 OK response with minimal latency.

**Acceptance Scenarios**:

1. **Given** the API is running, **When** a GET request is sent to the health check endpoint, **Then** the response is HTTP 200 OK
2. **Given** the API is running, **When** a GET request is sent to the health check endpoint, **Then** the response body indicates healthy status

---

### User Story 2 - Dependency Readiness Probe (Priority: P2)

Operations teams can verify that all critical backend dependencies (PostgreSQL, MongoDB, Keycloak) are reachable and functional.

**Why this priority**: Extends basic liveness to meaningful readiness — confirms the API can actually serve requests with its data stores connected.

**Independent Test**: Can be tested by checking the health endpoint while each dependency is individually available or unavailable, verifying accurate status reporting.

**Acceptance Scenarios**:

1. **Given** all dependencies are reachable, **When** a GET request is sent to the readiness endpoint, **Then** the response indicates all dependencies are healthy
2. **Given** one or more dependencies are unreachable, **When** a GET request is sent to the readiness endpoint, **Then** the response indicates degraded status with details of failed dependencies

---

### User Story 3 - Health Status Reporting (Priority: P3)

The health check response includes structured data describing each dependency's status for monitoring dashboards and alerting systems.

**Why this priority**: Enables automated alerting and observability integration, allowing teams to react proactively to infrastructure issues.

**Independent Test**: Can be tested by parsing the health check JSON response and validating that it contains named dependency entries with pass/fail indicators.

**Acceptance Scenarios**:

1. **Given** a successful health check, **When** the response is parsed, **Then** each dependency has a name and status field
2. **Given** a failed dependency, **When** the response is parsed, **Then** the failure includes descriptive information for troubleshooting

---

### Edge Cases

- Dependency checks exceeding 3 seconds per dependency are marked unhealthy with a "timeout" failure category
- Transient network blips within the 3-second window result in retry until timeout; sustained failures beyond 3 seconds trigger HTTP 503
- MongoDB config retrieval failures after Polly retries exhausted are reported as "connection refused" or "timeout" depending on the root cause

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST expose a liveness endpoint that returns HTTP 200 when the API process is running
- **FR-002**: System MUST expose a readiness endpoint that validates connectivity to all critical backend dependencies
- **FR-003**: System MUST report PostgreSQL database connectivity status in health check responses
- **FR-004**: System MUST report MongoDB configuration store connectivity status in health check responses
- **FR-005**: System MUST return HTTP 200 only when ALL dependencies (PostgreSQL, MongoDB, Keycloak) are healthy, and HTTP 503 when any single dependency is degraded
- **FR-006**: Health check endpoints MUST be accessible without authentication for orchestrator probes
- **FR-007**: Failed dependency responses MUST include the dependency name and a failure category (e.g., "timeout", "connection refused") without raw error messages

### Key Entities *(include if feature involves data)*

- **Health Check Result**: Represents the aggregate status of the API and its dependencies, including individual dependency statuses and an overall health determination

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Liveness endpoint responds within 200 milliseconds under normal conditions
- **SC-002**: Readiness endpoint accurately reflects dependency status with individual dependency checks completing within 3 seconds and total latency under 10 seconds
- **SC-003**: Health check endpoints achieve 99.9% availability during normal operations
- **SC-004**: Orchestrator probes can distinguish between process alive and fully ready states

## Assumptions

- Container orchestrators (e.g., Kubernetes, Podman Compose) use standard HTTP health probe patterns
- Health check endpoints are public-facing for infrastructure tooling; no user authentication required
- Dependency connectivity checks use existing connection strings and configuration already loaded at startup
- MongoDB Polly retry policy behavior applies to health check queries against the config store
