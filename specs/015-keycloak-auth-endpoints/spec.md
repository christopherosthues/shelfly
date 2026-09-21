# Feature Specification: Keycloak Authentication Endpoints

**Feature Branch**: `[015-keycloak-auth-endpoints]`

**Created**: 2026-09-20

**Status**: Draft

**Input**: User description: "Based on the shelfly-api.yaml OpenAPI description, create the endpoints for authentication. The actual account is managed and maintained by Keycloak."

## Clarifications

### Session 2026-09-20

- Q: Should login attempts be rate-limited to prevent brute-force attacks, and if so, what threshold should trigger account lockout? → A: 5 failed attempts per minute, then temporary lockout after 10 failures within 15 minutes
- Q: Is password reset (e.g., forgot password flow) in scope for this authentication feature, or should it be deferred to a future iteration? → A: Include password reset endpoint in this feature (adds POST /v1/auth/reset-password)
- Q: Should the API emit structured observability signals (logs, metrics, traces) for authentication events like login success/failure, registration, and token refresh? → A: Full distributed tracing with OpenTelemetry spans for each auth endpoint
- Q: Should the API enforce a specific JWT access token lifetime (e.g., 15 minutes) or rely entirely on Keycloak's default token configuration? → A: Use Keycloak default token configuration (admin-adjustable via realm settings)
- Q: Should the registration endpoint require email verification (e.g., confirmation link sent by Keycloak) before the account becomes fully active? → A: Skip verification for v1; account active immediately on registration

## User Scenarios & Testing *(mandatory)*

### User Story 1 - New User Registration (Priority: P1)

A new user provides an email address and password to create a Shelfly account. The system creates the corresponding Keycloak user and returns confirmation with the assigned user identifier.

**Why this priority**: Registration is the gateway entry point — without it, no other authentication flow can begin. It establishes the foundational user identity for all subsequent operations.

**Independent Test**: Can be fully tested by submitting a valid email/password pair and verifying that a new Keycloak account exists with the returned user ID.

**Acceptance Scenarios**:

1. **Given** a unique email address and a password meeting minimum strength requirements, **When** the user submits a registration request, **Then** the system creates a Keycloak account and returns 201 with the new user identifier
2. **Given** an email already registered in Keycloak, **When** the user submits a registration request, **Then** the system returns 409 Conflict indicating the email is taken
3. **Given** a password shorter than eight characters, **When** the user submits a registration request, **Then** the system returns 400 with validation error details

---

### User Story 2 - User Login (Priority: P1)

An existing user provides their email and password to authenticate against Keycloak. Upon success, the system issues an access token (JWT) and refresh token for subsequent API calls.

**Why this priority**: Login is the primary authentication mechanism enabling all protected resource access. Every authenticated session depends on this flow.

**Independent Test**: Can be fully tested by logging in with known credentials and verifying that a valid JWT access token and refresh token are returned, which can then authenticate subsequent requests.

**Acceptance Scenarios**:

1. **Given** valid email and password for an existing Keycloak user, **When** the user submits a login request, **Then** the system returns 200 with an access token, refresh token, token type, and expiration duration
2. **Given** correct email but wrong password, **When** the user submits a login request, **Then** the system returns 401 Unauthorized
3. **Given** missing or malformed fields in the login payload, **When** the user submits a login request, **Then** the system returns 400 with validation error details

---

### User Story 3 - Token Refresh (Priority: P2)

An authenticated user whose access token has expired uses their refresh token to obtain a new access token without re-entering credentials.

**Why this priority**: Token refresh extends session continuity seamlessly, reducing friction for users who would otherwise need to log in repeatedly during extended reading sessions.

**Independent Test**: Can be fully tested by obtaining a login response, waiting (or simulating) token expiration, and using the refresh token to acquire a fresh access token.

**Acceptance Scenarios**:

1. **Given** a valid refresh token from a prior login, **When** the user submits a refresh request, **Then** the system returns 200 with a new access token and refresh token
2. **Given** an expired or revoked refresh token, **When** the user submits a refresh request, **Then** the system returns 401 Unauthorized
3. **Given** a missing or malformed refresh token in the payload, **When** the user submits a refresh request, **Then** the system returns 400 with validation error details

---

### User Story 4 - Password Reset (Priority: P2)

A user who forgot their password submits their email address to initiate a reset. The system sends a Keycloak-generated reset link and, upon completion, allows the user to log in with the new credentials.

**Why this priority**: Password recovery is essential for account accessibility but ranks below initial registration and login as not every user will need it immediately.

**Independent Test**: Can be fully tested by submitting a registered email, receiving a reset confirmation, and verifying that subsequent login succeeds with the new password.

**Acceptance Scenarios**:

1. **Given** a valid registered email address, **When** the user submits a password reset request, **Then** the system returns 200 confirming a reset link was sent
2. **Given** an unregistered email address, **When** the user submits a password reset request, **Then** the system returns 404 Not Found or 200 with a generic confirmation (to avoid email enumeration)
3. **Given** missing or malformed email in the payload, **When** the user submits a password reset request, **Then** the system returns 400 with validation error details

---

### Edge Cases

- What happens when Keycloak is temporarily unreachable (network failure, maintenance)? The API should return 500 and retry logic should be configurable.
- How does the system handle concurrent login attempts from multiple devices using the same credentials? Each device receives independent tokens.
- What happens if a user registers with an email that differs only in case sensitivity? Keycloak's default behavior applies (case-sensitive by default).
- What happens when rate limits are exceeded? The API returns 429 Too Many Requests with retry-after guidance, and after 10 failures within 15 minutes the account is temporarily locked.
- Should password reset return 404 for unregistered emails or a generic 200 to avoid email enumeration? A generic 200 confirmation avoids revealing whether an email exists in Keycloak.

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST accept POST requests to `/v1/auth/register` with email and password, delegating account creation to Keycloak
- **FR-002**: System MUST validate email format and minimum password length (8 characters) before forwarding to Keycloak
- **FR-003**: System MUST return 409 Conflict when the email address already exists in Keycloak
- **FR-004**: System MUST accept POST requests to `/v1/auth/login` with email and password, authenticating against Keycloak
- **FR-005**: System MUST return JWT access token and refresh token upon successful login (200)
- **FR-006**: System MUST return 401 Unauthorized when credentials are invalid in Keycloak
- **FR-007**: System MUST accept POST requests to `/v1/auth/refresh` with a valid refresh token, issuing new access tokens via Keycloak
- **FR-008**: System MUST return 401 Unauthorized for expired or revoked refresh tokens
- **FR-009**: All error responses MUST follow RFC 7807 Problem Details format
- **FR-010**: System MUST delegate all user lifecycle operations (registration, login, token management) to Keycloak without local account storage
- **FR-011**: System MUST rate-limit login attempts: maximum 5 failed attempts per minute, with temporary lockout after 10 failures within a 15-minute window
- **FR-012**: System MUST accept POST requests to `/v1/auth/reset-password` with an email address, delegating password reset initiation to Keycloak
- **FR-013**: System MUST return 404 Not Found when the email address is not registered in Keycloak (or return 200 with generic confirmation to avoid email enumeration)
- **FR-014**: System MUST emit OpenTelemetry spans for each authentication endpoint covering registration, login, token refresh, and password reset events, including user ID, request ID, and outcome correlation

### Key Entities *(include if feature involves data)*

- **Keycloak User**: Managed entirely by Keycloak; identified by email address and UUID. The API stores no local copy of the account — it acts as a thin delegation layer.
- **JWT Access Token**: Issued by Keycloak, validated by the API using JWKS discovery. Contains audience (`aud`) claim validated against configured audience.
- **Refresh Token**: Issued by Keycloak, used to obtain new access tokens without re-authentication.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can complete registration in under 5 seconds (end-to-end including Keycloak response)
- **SC-002**: Users can authenticate and receive valid tokens within 3 seconds of submitting credentials
- **SC-003**: Token refresh completes within 2 seconds, maintaining session continuity
- **SC-004**: 95% of authentication requests return a response (success or error) within 10 seconds

## Assumptions

- Keycloak is deployed and accessible at the configured URL with the default realm or a Shelfly-specific realm
- Keycloak uses email as the username for user accounts
- Email verification is skipped for v1; newly registered accounts are immediately active and can log in without clicking a confirmation link
- Password policy enforces minimum length of 8 characters; additional complexity rules are configurable in Keycloak
- The API has administrative access to Keycloak via a dedicated service account or client credentials for user management operations
- JWT tokens issued by Keycloak include standard claims (sub, aud, exp, iat) and the API validates these against JWKS discovery endpoints
- Token lifetime follows Keycloak realm defaults; no API-level override is applied to access token expiration
- Network connectivity between the API and Keycloak is stable; transient failures result in 500 responses with retry-after guidance
