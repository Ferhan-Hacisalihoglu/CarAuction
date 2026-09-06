@echo off
setlocal EnableDelayedExpansion

title CarAuction - Full Stack Master Runner

cd /d "%~dp0"

set "ACTION=%~1"
if "%ACTION%"=="" set "ACTION=up"

echo ===============================================================================
echo                CARAUCTION - FULL STACK MASTER RUNNER
echo ===============================================================================
echo.

if /i "%ACTION%"=="down" (
    echo [*] Stopping Frontend services...
    call "%~dp0CarAuction.Frontend\runner.bat" down
    echo.
    echo [*] Stopping Backend services...
    call "%~dp0CarAuction.Backend\runner.bat" down
    echo.
    echo [*] All services stopped.
    goto :end
)

if /i "%ACTION%"=="restart" (
    echo [*] Restarting Backend services...
    call "%~dp0CarAuction.Backend\runner.bat" restart
    echo.
    echo [*] Restarting Frontend services...
    call "%~dp0CarAuction.Frontend\runner.bat" restart
    goto :status
)

REM Action: UP (Default)
echo [*] Step 1/2: Launching Backend Services (API + PostgreSQL 16 + Redis 7)...
call "%~dp0CarAuction.Backend\runner.bat" up

echo.
echo [*] Step 2/2: Launching Frontend Service (React 18 + Vite + Nginx)...
call "%~dp0CarAuction.Frontend\runner.bat" up

:status
echo.
echo ===============================================================================
echo                     ALL CARAUCTION SERVICES ACTIVE
echo ===============================================================================
echo   Frontend Web Application:  http://localhost:3000
echo   Backend REST API:          http://localhost:5000
echo   Swagger Documentation:     http://localhost:5000/swagger/index.html
echo   System Health Probe:       http://localhost:5000/health/ready
echo   PostgreSQL 16 Database:    localhost:5432 (carauction_db)
echo   Redis 7 Distributed Cache: localhost:6379
echo ===============================================================================
echo.
echo Quick commands:
echo   runner.bat down     - Stop all containers
echo   runner.bat restart  - Restart all containers
echo.

:end
if "%~1"=="" pause
