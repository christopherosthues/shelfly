# Health Check API Contract

**Date**: 2026-09-21

## Endpoints

### GET /v1/health/live

Verifies the API process is running and able to respond.

**Request**:
- Method: `GET`
- URL: `/v1/health/live`
- Authentication: None (public endpoint for orchestrator probes)

**Response — Success (HTTP 200)**:

```json
{
  "Status": "Healthy",
  "Dependencies": [],
  "Timestamp": "2026-09-21T14:30:00Z"
}
```

**Performance Target**: <200ms latency (SC-001)

---

### GET /v1/health/ready

Validates connectivity to all backend dependencies (PostgreSQL, MongoDB, Keycloak).

**Request**:
- Method: `GET`
- URL: `/v1/health/ready`
- Authentication: None (public endpoint for orchestrator probes)

**Response — All Healthy (HTTP 200)**:

```json
{
  "Status": "Healthy",
  "Dependencies": [
    {
      "Name": "PostgreSQL",
      "Status": "Healthy",
      "FailureCategory": null,
      "Duration": "00:00:00.1500000"
    },
    {
      "Name": "MongoDB",
      "Status": "Healthy",
      "FailureCategory": null,
      "Duration": "00:00:00.2000000"
    },
    {
      "Name": "Keycloak",
      "Status": "Healthy",
      "FailureCategory": null,
      "Duration": "00:00:00.3000000"
    }
  ],
  "Timestamp": "2026-09-21T14:30:00Z"
}
```

**Response — Dependency Failed (HTTP 503)**:

```json
{
  "Status": "Unhealthy",
  "Dependencies": [
    {
      "Name": "PostgreSQL",
      "Status": "Healthy",
      "FailureCategory": null,
      "Duration": "00:00:00.1500000"
    },
    {
      "Name": "MongoDB",
      "Status": "Unhealthy",
      "FailureCategory": "timeout",
      "Duration": "00:00:03.0000000"
    },
    {
      "Name": "Keycloak",
      "Status": "Healthy",
      "FailureCategory": null,
      "Duration": "00:00:00.2500000"
    }
  ],
  "Timestamp": "2026-09-21T14:30:03Z"
}
```

**Performance Target**: <10s total latency; 3s per-dependency timeout (SC-002)

## Failure Categories

| Category | Meaning | Example Causes |
|----------|---------|----------------|
| `timeout` | Dependency did not respond within 3 seconds | Network latency, server overload, connection pool exhaustion |
| `connection refused` | TCP connection failed or was rejected | Service down, wrong port, firewall blocking |
| `other` | Unexpected error during check | Driver exception, configuration error |

## HTTP Status Code Mapping

| Health Status | HTTP Status | Description |
|---------------|-------------|-------------|
| Healthy | 200 OK | All checks passed |
| Unhealthy | 503 Service Unavailable | One or more dependencies failed |
