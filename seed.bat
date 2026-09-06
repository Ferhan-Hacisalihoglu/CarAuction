@echo off
setlocal
cd /d "%~dp0"

echo ===============================================================================
echo                     CARAUCTION - DATABASE SEED RUNNER
echo ===============================================================================
echo.

call "%~dp0CarAuction.Backend\runner.bat" seed

if "%~1"=="" pause
