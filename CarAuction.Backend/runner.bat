@echo off
setlocal EnableDelayedExpansion

title CarAuction - Backend Runner

cd /d "%~dp0"

echo ===============================================================================
echo                     CARAUCTION - BACKEND DOCKER RUNNER
echo ===============================================================================
echo.

set "ACTION=%~1"
if "%ACTION%"=="" set "ACTION=up"

if /i "%ACTION%"=="down" (
    echo [*] Stopping and removing backend containers...
    docker compose down
    echo [*] Backend stopped.
    goto :end
)

if /i "%ACTION%"=="restart" (
    echo [*] Restarting backend containers...
    docker compose restart
    goto :status
)

if /i "%ACTION%"=="logs" (
    echo [*] Streaming backend logs...
    docker compose logs -f
    goto :end
)

REM 1. Ensure shared docker network exists
echo [*] Checking shared network: carauction_network...
docker network inspect carauction_network >nul 2>&1
if errorlevel 1 (
    echo [+] Creating shared network: carauction_network...
    docker network create carauction_network >nul
) else (
    echo [OK] Shared network exists.
)

REM 2. Build and start containers
echo [*] Starting Backend, PostgreSQL 16, and Redis 7 containers...
docker compose up --build -d

if errorlevel 1 (
    echo.
    echo [!] ERROR: Failed to start backend services.
    goto :end
)

:status
echo.
echo ===============================================================================
echo                       BACKEND SERVICES RUNNING
echo ===============================================================================
echo   API Base URL:     http://localhost:5000
echo   Swagger UI:       http://localhost:5000/swagger/index.html
echo   Health Probe:     http://localhost:5000/health/ready
echo   PostgreSQL 16:    localhost:5432 (Database: carauction_db)
echo   Redis 7 Cache:    localhost:6379
echo ===============================================================================
echo.
docker compose ps
echo.
echo Tip: Use 'runner.bat down' to stop, 'runner.bat logs' to stream logs.
echo.

:end
if "%~1"=="" pause
