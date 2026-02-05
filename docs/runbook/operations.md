# Operations Runbook

## Starting the Application

### Docker Compose (Production-like)

```bash
cd build/docker
docker-compose up -d
```

### Local Development

```bash
# Windows
.\build\scripts\dev.ps1 start

# Linux/macOS
./build/scripts/dev.sh start
```

## Stopping the Application

```bash
# Docker
docker-compose down

# Local
.\build\scripts\dev.ps1 stop
```

## Viewing Logs

### Docker Logs
```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
```

### Application Logs
- **Console**: stdout of running process
- **Files**: `logs/apihub-api-{date}.log`
- **Seq**: http://localhost:5341

## Database Operations

### Run Migrations
```bash
cd src/ApiHub.Api
dotnet ef database update
```

### Create Migration
```bash
cd src/ApiHub.Api
dotnet ef migrations add MigrationName
```

### Reset Database
```bash
dotnet ef database drop --force
dotnet ef database update
```

### Backup PostgreSQL
```bash
docker exec apihub-postgres pg_dump -U postgres apihub > backup.sql
```

### Restore PostgreSQL
```bash
docker exec -i apihub-postgres psql -U postgres apihub < backup.sql
```

## Health Checks

### Endpoints
- `/health` - Full health check
- `/health/ready` - Readiness probe
- `/health/live` - Liveness probe

### Check Health
```bash
curl http://localhost:5001/health
```

## Common Issues

### PostgreSQL Connection Failed
1. Check if container is running: `docker ps`
2. Check container logs: `docker logs apihub-postgres`
3. Verify connection string in appsettings.json
4. Ensure port 5432 is not in use

### Redis Connection Failed
1. Check if Redis container is running
2. Try connecting: `redis-cli ping`
3. Application will fall back to in-memory cache

### JWT Token Issues
1. Verify JWT secret is at least 32 characters
2. Check token expiration
3. Ensure clock sync between servers

### Migration Errors
1. Check EF Core tools are installed
2. Verify connection string
3. Review migration files for conflicts

## Monitoring

### Key Metrics
- Request rate per endpoint
- Error rate (4xx, 5xx)
- Response latency (p50, p95, p99)
- Active connections
- Database query time

### Alerts to Configure
- Error rate > 5%
- p99 latency > 2s
- Database connection failures
- Circuit breaker opened

## Scaling

### Horizontal Scaling (API)
1. Ensure session state is in Redis
2. Use load balancer with sticky sessions disabled
3. Scale API instances independently

### Vertical Scaling
- API: CPU bound for JSON processing
- Database: I/O bound, increase RAM for caching

## Security Operations

### Rotate JWT Secret
1. Update `Jwt:Secret` in configuration
2. Restart all API instances
3. Users will need to re-authenticate

### Rotate Encryption Key
1. Decrypt existing secrets
2. Update `Encryption:Key`
3. Re-encrypt secrets
4. Update database

### Review Audit Logs
```sql
SELECT * FROM "AuditLogs"
WHERE "Action" IN (0, 1, 50, 51, 52)  -- Auth and user actions
ORDER BY "CreatedAt" DESC
LIMIT 100;
```

## Performance Tuning

### Database
- Add indexes for frequent queries
- Enable connection pooling (default in Npgsql)
- Vacuum analyze regularly

### Caching
- Configure Redis maxmemory policy
- Adjust cache TTL based on data freshness needs

### API
- Enable response compression
- Tune Polly retry policies
- Adjust rate limits based on load testing
