# Quickstart Validation Guide: API Health Checks

**Date**: 2026-09-21

## Prerequisites

1. **MongoDB running**: Either local instance or via `docker compose up mongodb`
2. **PostgreSQL running**: Via `docker compose up postgresql` (for readiness tests)
3. **Keycloak configured**: Base URL and realm in `appsettings.json` or environment variables
4. **Connection strings**: MongoDB connection string in configuration (`MongoDb` key)

## Setup Commands

```bash
# Start infrastructure services
docker compose up mongodb postgresql -d

# Run the API (development mode)
dotnet run --project Shelfly.Api
```

## Validation Scenarios

### Scenario 1: Liveness Probe — Process Running

**Command**:
```bash
curl http://localhost:5000/v1/health/live
```

**Expected Outcome**:
- HTTP Status: `200 OK`
- Response body contains `"Status": "Healthy"`
- Latency: <200ms

---

### Scenario 2: Readiness Probe — All Dependencies Healthy

**Command**:
```bash
curl http://localhost:5000/v1/health/ready
```

**Expected Outcome**:
- HTTP Status: `200 OK`
- Response body contains `"Status": "Healthy"`
- All three dependencies (PostgreSQL, MongoDB, Keycloak) show `"Status": "Healthy"`
- Latency: <10s total

---

### Scenario 3: Readiness Probe — Dependency Timeout

**Setup**: Stop one dependency (e.g., `docker compose stop mongodb`)

**Command**:
```bash
curl -v http://localhost:5000/v1/health/ready
```

**Expected Outcome**:
- HTTP Status: `503 Service Unavailable`
- Response body contains `"Status": "Unhealthy"`
- Failed dependency shows `"FailureCategory": "timeout"` or `"connection refused"`
- Latency: ~3s (per-dependency timeout) + overhead

---

### Scenario 4: Readiness Probe — Multiple Failures

**Setup**: Stop MongoDB and PostgreSQL

**Command**:
```bash
curl -v http://localhost:5000/v1/health/ready
```

**Expected Outcome**:
- HTTP Status: `503 Service Unavailable`
- Response body lists all dependencies with individual status
- Each failed dependency includes a failure category

## Reference Documents

- **API Contract**: [contracts/health-endpoints.md](./contracts/health-endpoints.md)
- **Data Model**: [data-model.md](./data-model.md)
- **Research Decisions**: [research.md](./research.md)
