#!/bin/bash

# Development script for Linux/macOS
set -e

PROJECT_ROOT="$(cd "$(dirname "$0")/../.." && pwd)"
COMMAND=${1:-start}

start_dev() {
    echo -e "\033[32mStarting development environment...\033[0m"

    # Start Docker containers
    cd "$PROJECT_ROOT/build/docker"
    docker-compose up -d postgres redis seq

    echo -e "\033[33mWaiting for PostgreSQL to be ready...\033[0m"
    sleep 5

    # Run migrations
    cd "$PROJECT_ROOT/src/ApiHub.Api"
    dotnet ef database update

    echo -e "\033[32mStarting API...\033[0m"
    dotnet run --project "$PROJECT_ROOT/src/ApiHub.Api/ApiHub.Api.csproj" &

    echo -e "\033[32mStarting Web...\033[0m"
    dotnet run --project "$PROJECT_ROOT/src/ApiHub.Web/ApiHub.Web.csproj" &

    echo ""
    echo -e "\033[32mDevelopment environment started!\033[0m"
    echo -e "\033[36mAPI: https://localhost:5001\033[0m"
    echo -e "\033[36mWeb: https://localhost:5002\033[0m"
    echo -e "\033[36mSwagger: https://localhost:5001/swagger\033[0m"
    echo -e "\033[36mSeq (Logs): http://localhost:5341\033[0m"
}

stop_dev() {
    echo -e "\033[33mStopping development environment...\033[0m"

    # Stop .NET processes
    pkill -f "dotnet run" || true

    # Stop Docker containers
    cd "$PROJECT_ROOT/build/docker"
    docker-compose down

    echo -e "\033[32mDevelopment environment stopped.\033[0m"
}

show_logs() {
    cd "$PROJECT_ROOT/build/docker"
    docker-compose logs -f
}

build_solution() {
    echo -e "\033[32mBuilding solution...\033[0m"
    dotnet build "$PROJECT_ROOT/ApiHubSystem.sln" -c Debug
}

run_tests() {
    echo -e "\033[32mRunning tests...\033[0m"
    dotnet test "$PROJECT_ROOT/ApiHubSystem.sln" --verbosity normal
}

run_migrations() {
    echo -e "\033[32mRunning database migrations...\033[0m"
    cd "$PROJECT_ROOT/src/ApiHub.Api"
    dotnet ef database update
}

case $COMMAND in
    start) start_dev ;;
    stop) stop_dev ;;
    restart) stop_dev && start_dev ;;
    logs) show_logs ;;
    build) build_solution ;;
    test) run_tests ;;
    migrate) run_migrations ;;
    *) echo -e "\033[31mUnknown command: $COMMAND\033[0m" ;;
esac
