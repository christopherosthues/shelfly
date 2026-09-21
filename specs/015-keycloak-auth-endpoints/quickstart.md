# Quickstart: Authentication Endpoints Validation Guide

**Date**: 2026-09-20  
**Feature**: 015-keycloak-auth-endpoints

## Prerequisites

### Infrastructure

1. **Docker / Podman** installed and running
2. **Environment files** configured in `envfiles/`:
   - `keycloak.env` — Keycloak instance configuration
   - `shelfly.env` — API service configuration
   - `mongo.env` — MongoDB connection details

### Keycloak Setup

Before running validation scenarios, ensure Keycloak is properly configured:

1. **Realm created**: A dedicated realm for Shelfly exists (e.g., `shelfly`)
2. **Service account client**: Confidential client with `service-account` enabled and appropriate roles mapped (`realm-admin`, `view-users`, `manage-users`)
3. **Email server configured**: SMTP settings in Keycloak admin console for password reset flow
4. **User federation** (optional): If using external identity providers, configure accordingly

### Local Development

```bash
# Start infrastructure stack
docker compose up -d keycloak postgres mongo

# Verify services are running
docker compose ps

# Seed MongoDB configuration (if first run)
dotnet run --project Shelfly.AdminConsole seed-config
```

## Validation Scenarios

### Scenario 1: User Registration

**Goal**: Verify new user can register successfully via the API.

**Steps**:

1. **Send registration request**:
   ```bash
   curl -X POST http://localhost:5000/v1/auth/register \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test.user@example.com",
       "password": "SecurePass123!",
       "firstName": "Test",
       "lastName": "User"
     }'
   ```

2. **Expected response** (201 Created):
   ```json
   {
     "userId": "<uuid-v7>",
     "email": "test.user@example.com",
     "status": "active"
   }
   ```

3. **Verify in Keycloak**: Check that the user exists in the target realm via admin console or API:
   ```bash
   curl -H "Authorization: Bearer <admin-token>" \
     http://localhost:8080/admin/realms/shelfly/users | jq '.[] | select(.email == "test.user@example.com")'
   ```

**Success Criteria**:
- [ ] HTTP 201 response received with valid user ID
- [ ] User exists in Keycloak realm
- [ ] Duplicate registration returns 409 Conflict

### Scenario 2: User Login

**Goal**: Verify registered user can authenticate and receive JWT tokens.

**Steps**:

1. **Send login request**:
   ```bash
   curl -X POST http://localhost:5000/v1/auth/login \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test.user@example.com",
       "password": "SecurePass123!"
     }'
   ```

2. **Expected response** (200 OK):
   ```json
   {
     "accessToken": "eyJhbGci...",
     "refreshToken": "dGhpcyBpcy...",
     "expiresIn": 3600,
     "tokenType": "Bearer"
   }
   ```

3. **Validate token**: Decode the JWT to verify claims:
   ```bash
   # Using jwt.io or similar tool to inspect token contents
   echo "<accessToken>" | jq -R 'split(".") | .[0], .[1] | @base64d | fromjson'
   ```

**Success Criteria**:
- [ ] HTTP 200 response with valid tokens
- [ ] Access token contains expected claims (sub, aud, exp)
- [ ] Invalid credentials return 401 Unauthorized
- [ ] Rate limiting headers present in response

### Scenario 3: Token Refresh

**Goal**: Verify refresh token can obtain new access token.

**Steps**:

1. **Send refresh request**:
   ```bash
   curl -X POST http://localhost:5000/v1/auth/refresh \
     -H "Content-Type: application/json" \
     -d '{
       "refreshToken": "<token-from-login>"
     }'
   ```

2. **Expected response** (200 OK):
   ```json
   {
     "accessToken": "eyJhbGci...",
     "refreshToken": "bmV3IHJlZn...",
     "expiresIn": 3600,
     "tokenType": "Bearer"
   }
   ```

**Success Criteria**:
- [ ] HTTP 200 response with new token pair
- [ ] New access token differs from original
- [ ] Expired refresh token returns 401 Unauthorized

### Scenario 4: Password Reset

**Goal**: Verify password reset flow initiates correctly.

**Steps**:

1. **Send reset request**:
   ```bash
   curl -X POST http://localhost:5000/v1/auth/reset-password \
     -H "Content-Type: application/json" \
     -d '{
       "email": "test.user@example.com"
     }'
   ```

2. **Expected response** (200 OK):
   ```json
   {
     "message": "Password reset link sent to test.user@example.com",
     "expiresIn": 900
   }
   ```

3. **Check email**: Verify Keycloak sent the reset link (check mail server logs or inbox)

**Success Criteria**:
- [ ] HTTP 200 response with confirmation message
- [ ] Email received containing reset link
- [ ] Non-existent email returns 404 Not Found

### Scenario 5: Rate Limiting

**Goal**: Verify login rate limiting enforces thresholds.

**Steps**:

1. **Send rapid login attempts** (use invalid password):
   ```bash
   for i in {1..12}; do
     curl -s -w "\nHTTP %{http_code}\n" \
       -X POST http://localhost:5000/v1/auth/login \
       -H "Content-Type: application/json" \
       -d '{
         "email": "test.user@example.com",
         "password": "WrongPass123!"
       }'
   done
   ```

2. **Expected behavior**:
   - First 10 attempts return 401 Unauthorized with decreasing `X-RateLimit-Remaining`
   - 11th attempt returns 429 Too Many Requests
   - Subsequent attempts blocked until lockout expires (15 minutes)

**Success Criteria**:
- [ ] Rate limit headers present in all responses
- [ ] Lockout enforced after threshold exceeded
- [ ] Retry-After header indicates unlock time

## Observability Validation

### OpenTelemetry Traces

**Goal**: Verify authentication events are traced correctly.

**Steps**:

1. **Execute any auth endpoint** (e.g., login)
2. **Query trace collector** (e.g., Jaeger, Zipkin):
   ```bash
   # Query traces for the operation
   curl "http://localhost:16686/api/traces?service=shelfly-api&operation=/v1/auth/login" | jq '.[].traceId'
   ```

3. **Verify trace contains**:
   - Span for endpoint execution
   - Child span for Keycloak Admin API call
   - Attributes: user ID, operation type, success/failure status

**Success Criteria**:
- [ ] Trace visible in collector UI
- [ ] Distributed context propagated to Keycloak calls
- [ ] Custom attributes captured (user context, operation metadata)

## Cleanup

After validation, clean up test data:

```bash
# Delete test user from Keycloak
curl -X DELETE \
  -H "Authorization: Bearer <admin-token>" \
  http://localhost:8080/admin/realms/shelfly/users/<userId>

# Clear rate limit entries (if MongoDB accessible)
mongosh --eval 'db.rateLimitEntries.deleteMany({ userId: "<test-user-id>" })'

# Stop infrastructure stack
docker compose down
```

## References

- [API Contract](./contracts/auth-api.md) — Full endpoint specifications
- [Data Model](./data-model.md) — Entity definitions and relationships
- [Research Notes](./research.md) — Technical decisions and rationale
