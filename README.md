# ApiHubSystem

A production-grade web application for connecting to public APIs, running requests, storing audit logs, analyzing data, and generating reports.

## Features

- **User Authentication**: Register, login, password reset with roles (Admin, Analyst, Viewer)
- **API Connectors**: Pre-configured connectors for JSONPlaceholder, Petstore, ReqRes, GitHub, OpenWeather
- **API Runner**: Send GET/POST/PUT/PATCH/DELETE requests with custom headers and body
- **API Records**: Full audit log of all requests with correlation IDs
- **Analytics Dashboard**: Charts, metrics, latency percentiles, error rates
- **Reports**: Generate PDF and Excel reports with scheduling
- **File Upload**: Upload CSV/JSON files, parse and visualize data
- **Resilience**: Retry, timeout, circuit breaker with Polly
- **API Versioning**: /api/v1 and /api/v2 endpoints
- **Health Checks**: Comprehensive health monitoring

## Tech Stack

- **.NET 10** (LTS) + **ASP.NET Core**
- **Blazor Web App** (Server-side)
- **Entity Framework Core** with PostgreSQL
- **ASP.NET Core Identity** + JWT Authentication
- **MediatR** + **FluentValidation** + **AutoMapper**
- **Polly** for resilience
- **Serilog** + **OpenTelemetry** for observability
- **QuestPDF** + **ClosedXML** for reports
- **Docker** + **GitHub Actions** for CI/CD

## Project Structure

```
ApiHubSystem/
├── src/
│   ├── ApiHub.Web/           # Blazor Web Application
│   ├── ApiHub.Api/           # REST API
│   ├── ApiHub.Application/   # Business Logic (CQRS)
│   ├── ApiHub.Domain/        # Entities & Domain Rules
│   ├── ApiHub.Infrastructure/# EF Core, External APIs, Services
│   └── ApiHub.Shared/        # Shared Utilities
├── tests/
│   ├── ApiHub.UnitTests/
│   ├── ApiHub.IntegrationTests/
│   ├── ApiHub.ContractTests/
│   └── ApiHub.UiTests/
├── build/
│   ├── docker/
│   └── scripts/
├── docs/
└── .github/workflows/
```

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [PostgreSQL](https://www.postgresql.org/) (or use Docker)

### Option 1: Using Docker Compose (Recommended)

```bash
cd build/docker
docker-compose up -d
```

Access:
- Web UI: http://localhost:5002
- API: http://localhost:5001
- Swagger: http://localhost:5001/swagger
- Seq Logs: http://localhost:5341

### Option 2: Local Development

**Windows (PowerShell):**
```powershell
.\build\scripts\dev.ps1 start
```

**Linux/macOS:**
```bash
chmod +x ./build/scripts/dev.sh
./build/scripts/dev.sh start
```

### Option 3: Manual Setup

1. Start PostgreSQL and Redis:
```bash
docker run -d --name postgres -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=apihub -p 5432:5432 postgres:16-alpine
docker run -d --name redis -p 6379:6379 redis:7-alpine
```

2. Run migrations:
```bash
cd src/ApiHub.Api
dotnet ef database update
```

3. Start the API:
```bash
cd src/ApiHub.Api
dotnet run
```

4. Start the Web app:
```bash
cd src/ApiHub.Web
dotnet run
```

## Default Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@apihub.local | Admin@123! |
| Analyst | analyst@apihub.local | Analyst@123! |
| Viewer | viewer@apihub.local | Viewer@123! |

## API Endpoints

### Authentication
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - Login and get JWT token
- `GET /api/v1/auth/me` - Get current user info

### Connectors
- `GET /api/v1/connectors` - List all connectors
- `POST /api/v1/connectors` - Create connector (Admin)

### API Runner
- `POST /api/v1/apirunner/send` - Send API request

### Records
- `GET /api/v1/records` - Get API records with filtering

### Analytics
- `GET /api/v1/analytics/dashboard` - Get dashboard data

### Reports
- `POST /api/v1/reports/generate` - Generate report

### Uploads
- `POST /api/v1/uploads` - Upload file

## Configuration

Key settings in `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=apihub;Username=postgres;Password=postgres",
    "Redis": "localhost:6379"
  },
  "Jwt": {
    "Secret": "YourSecretKey",
    "Issuer": "ApiHubSystem",
    "Audience": "ApiHubUsers",
    "ExpirationMinutes": "60"
  }
}
```

## Running Tests

```bash
# All tests
dotnet test

# Unit tests only
dotnet test tests/ApiHub.UnitTests

# Integration tests (requires Docker)
dotnet test tests/ApiHub.IntegrationTests

# Contract tests
dotnet test tests/ApiHub.ContractTests
```

## External APIs

| API | Base URL | Auth | Notes |
|-----|----------|------|-------|
| JSONPlaceholder | jsonplaceholder.typicode.com | None | CRUD demo |
| Swagger Petstore | petstore.swagger.io/v2 | API Key | CRUD demo |
| ReqRes | reqres.in/api | None | Test flows |
| GitHub | api.github.com | Bearer Token | User provides token |
| OpenWeather | api.openweathermap.org | API Key | User provides key |

## Architecture

The project follows **Clean Architecture** principles:

- **Domain Layer**: Entities, value objects, domain events, exceptions
- **Application Layer**: Use cases (CQRS with MediatR), interfaces, validation
- **Infrastructure Layer**: EF Core, external API clients, services
- **Presentation Layer**: API controllers, Blazor pages

## Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License.
