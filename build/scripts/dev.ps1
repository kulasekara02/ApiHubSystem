# Development script for Windows
param(
    [Parameter(Position=0)]
    [ValidateSet("start", "stop", "restart", "logs", "build", "test", "migrate", "seed")]
    [string]$Command = "start"
)

$ErrorActionPreference = "Stop"
$ProjectRoot = Split-Path -Parent (Split-Path -Parent $PSScriptRoot)

function Start-Dev {
    Write-Host "Starting development environment..." -ForegroundColor Green

    # Start Docker containers
    Push-Location "$ProjectRoot\build\docker"
    docker-compose up -d postgres redis seq
    Pop-Location

    Write-Host "Waiting for PostgreSQL to be ready..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5

    # Run migrations
    Push-Location "$ProjectRoot\src\ApiHub.Api"
    dotnet ef database update
    Pop-Location

    Write-Host "Starting API..." -ForegroundColor Green
    Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "$ProjectRoot\src\ApiHub.Api\ApiHub.Api.csproj" -WorkingDirectory $ProjectRoot

    Write-Host "Starting Web..." -ForegroundColor Green
    Start-Process -FilePath "dotnet" -ArgumentList "run", "--project", "$ProjectRoot\src\ApiHub.Web\ApiHub.Web.csproj" -WorkingDirectory $ProjectRoot

    Write-Host ""
    Write-Host "Development environment started!" -ForegroundColor Green
    Write-Host "API: https://localhost:5001" -ForegroundColor Cyan
    Write-Host "Web: https://localhost:5002" -ForegroundColor Cyan
    Write-Host "Swagger: https://localhost:5001/swagger" -ForegroundColor Cyan
    Write-Host "Seq (Logs): http://localhost:5341" -ForegroundColor Cyan
}

function Stop-Dev {
    Write-Host "Stopping development environment..." -ForegroundColor Yellow

    # Stop .NET processes
    Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force

    # Stop Docker containers
    Push-Location "$ProjectRoot\build\docker"
    docker-compose down
    Pop-Location

    Write-Host "Development environment stopped." -ForegroundColor Green
}

function Show-Logs {
    Push-Location "$ProjectRoot\build\docker"
    docker-compose logs -f
    Pop-Location
}

function Build-Solution {
    Write-Host "Building solution..." -ForegroundColor Green
    dotnet build "$ProjectRoot\ApiHubSystem.sln" -c Debug
}

function Run-Tests {
    Write-Host "Running tests..." -ForegroundColor Green
    dotnet test "$ProjectRoot\ApiHubSystem.sln" --verbosity normal
}

function Run-Migrations {
    Write-Host "Running database migrations..." -ForegroundColor Green
    Push-Location "$ProjectRoot\src\ApiHub.Api"
    dotnet ef database update
    Pop-Location
}

switch ($Command) {
    "start" { Start-Dev }
    "stop" { Stop-Dev }
    "restart" { Stop-Dev; Start-Dev }
    "logs" { Show-Logs }
    "build" { Build-Solution }
    "test" { Run-Tests }
    "migrate" { Run-Migrations }
    default { Write-Host "Unknown command: $Command" -ForegroundColor Red }
}
