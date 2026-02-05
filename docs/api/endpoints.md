# API Endpoints Reference

Base URL: `https://localhost:5001/api/v1`

## Authentication

All endpoints except `/auth/register` and `/auth/login` require a valid JWT token.

Include the token in the `Authorization` header:
```
Authorization: Bearer <your-jwt-token>
```

---

## Auth Endpoints

### Register User
```
POST /auth/register
```

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe"
}
```

**Response (200):**
```json
{
  "userId": "uuid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe"
}
```

### Login
```
POST /auth/login
```

**Request Body:**
```json
{
  "email": "user@example.com",
  "password": "Password123!"
}
```

**Response (200):**
```json
{
  "userId": "uuid",
  "email": "user@example.com",
  "firstName": "John",
  "lastName": "Doe",
  "accessToken": "jwt-token",
  "refreshToken": "refresh-token",
  "roles": ["Viewer"]
}
```

### Get Current User
```
GET /auth/me
```

**Response (200):**
```json
{
  "userId": "uuid",
  "email": "user@example.com",
  "name": "user@example.com",
  "roles": ["Viewer"]
}
```

---

## Connectors Endpoints

### List Connectors
```
GET /connectors
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number (default: 1) |
| pageSize | int | Items per page (default: 10) |
| searchTerm | string | Search by name/description |
| status | enum | Filter by status (Active/Inactive/Error) |

**Response (200):**
```json
{
  "items": [
    {
      "id": "uuid",
      "name": "JSONPlaceholder",
      "description": "Free fake REST API",
      "baseUrl": "https://jsonplaceholder.typicode.com",
      "authType": "None",
      "status": "Active",
      "isPublic": true,
      "endpointCount": 7,
      "createdAt": "2025-01-01T00:00:00Z"
    }
  ],
  "pageNumber": 1,
  "totalPages": 1,
  "totalCount": 5
}
```

### Create Connector (Admin Only)
```
POST /connectors
```

**Request Body:**
```json
{
  "name": "My API",
  "description": "Description",
  "baseUrl": "https://api.example.com",
  "authType": "ApiKey",
  "apiKeyHeaderName": "X-API-Key",
  "timeoutSeconds": 30,
  "maxRetries": 3,
  "isPublic": true,
  "endpoints": [
    {
      "name": "Get Items",
      "path": "/items",
      "method": "GET"
    }
  ]
}
```

---

## API Runner Endpoints

### Send Request
```
POST /apirunner/send
```

**Request Body:**
```json
{
  "connectorId": "uuid",
  "endpoint": "/posts",
  "method": "GET",
  "headers": {
    "X-Custom-Header": "value"
  },
  "body": null,
  "queryParams": {
    "page": "1"
  },
  "saveAsDataset": false
}
```

**Response (200):**
```json
{
  "apiRecordId": "uuid",
  "statusCode": 200,
  "responseHeaders": {
    "Content-Type": "application/json"
  },
  "responseBody": "[{\"id\": 1, \"title\": \"...\"}]",
  "durationMs": 145,
  "isSuccess": true,
  "errorMessage": null,
  "datasetId": null
}
```

---

## Records Endpoints

### Get API Records
```
GET /records
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| pageNumber | int | Page number |
| pageSize | int | Items per page |
| connectorId | uuid | Filter by connector |
| method | enum | Filter by HTTP method |
| statusCode | int | Filter by status code |
| isSuccess | bool | Filter by success |
| fromDate | datetime | Start date |
| toDate | datetime | End date |
| searchTerm | string | Search URL or correlation ID |

---

## Analytics Endpoints

### Get Dashboard Data
```
GET /analytics/dashboard
```

**Query Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| fromDate | datetime | Start date |
| toDate | datetime | End date |
| connectorId | uuid | Filter by connector |

**Response (200):**
```json
{
  "totalRequests": 12847,
  "successfulRequests": 12616,
  "failedRequests": 231,
  "successRate": 98.2,
  "averageLatencyMs": 245.5,
  "statusCodeDistribution": [
    { "statusCode": 200, "count": 11500 }
  ],
  "requestsOverTime": [
    { "date": "2025-01-01", "successCount": 400, "failureCount": 10 }
  ],
  "topConnectors": [...],
  "latencyPercentiles": [
    { "percentile": "p50", "valueMs": 189 }
  ]
}
```

---

## Reports Endpoints

### Generate Report
```
POST /reports/generate
```

**Request Body:**
```json
{
  "name": "API Activity Report",
  "templateType": "Activity",
  "format": "PDF",
  "filters": {
    "fromDate": "2025-01-01T00:00:00Z",
    "toDate": "2025-01-31T23:59:59Z",
    "connectorId": null,
    "successOnly": false
  },
  "schedule": "None"
}
```

**Response:** Binary file download

---

## Uploads Endpoints

### Upload File
```
POST /uploads
Content-Type: multipart/form-data
```

**Form Data:**
- `file`: The file to upload (CSV, JSON, or image)

**Response (200):**
```json
{
  "fileId": "uuid",
  "fileName": "data.csv",
  "rowCount": 150,
  "columnCount": 5,
  "headers": ["id", "name", "email", "status", "created"],
  "previewJson": "[{...}]"
}
```

---

## Health Endpoints

### Full Health Check
```
GET /health
```

### Readiness Probe
```
GET /health/ready
```

### Liveness Probe
```
GET /health/live
```

---

## Error Responses

All endpoints return errors in this format:

```json
{
  "errors": ["Error message 1", "Error message 2"]
}
```

**Status Codes:**
- `400` - Bad Request (validation errors)
- `401` - Unauthorized (missing/invalid token)
- `403` - Forbidden (insufficient permissions)
- `404` - Not Found
- `500` - Internal Server Error
