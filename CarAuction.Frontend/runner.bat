@echo off
setlocal EnableDelayedExpansion

title CarAuction - Frontend Runner

cd /d "%~dp0"

echo ===============================================================================
echo                    CARAUCTION - FRONTEND DOCKER RUNNER
echo ===============================================================================
echo.

set "ACTION=%~1"
if "%ACTION%"=="" set "ACTION=up"

if /i "%ACTION%"=="down" (
    echo [*] Stopping and removing frontend container...
    docker compose down
    echo [*] Frontend stopped.
    goto :end
)

if /i "%ACTION%"=="restart" (
    echo [*] Restarting frontend container...
    docker compose restart
    goto :status
)

if /i "%ACTION%"=="logs" (
    echo [*] Streaming frontend logs...
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

REM 2. Build and start frontend container
echo [*] Building and launching Frontend container (React 18 + Vite + Nginx)...
docker compose up --build -d

if errorlevel 1 (
    echo.
    echo [!] ERROR: Failed to start frontend service.
    goto :end
)

:status
echo.
echo ===============================================================================
echo                       FRONTEND SERVICE RUNNING
echo ===============================================================================
echo   Web Application:   http://localhost:3000
echo   API Reverse Proxy: http://localhost:3000/api/   -^> carauction_backend:5000
echo   SignalR Hub Proxy: http://localhost:3000/hubs/  -^> carauction_backend:5000
echo ===============================================================================
echo.
docker compose ps
echo.
echo Tip: Use 'runner.bat down' to stop, 'runner.bat logs' to stream logs.
echo.

:end
if "%~1"=="" pause
