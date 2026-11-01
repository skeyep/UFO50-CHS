@echo off
setlocal
title UFO50-CHS Check
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0installer\Check-UFO50-CHS.ps1"
set "UFO50_CHS_EXIT=%ERRORLEVEL%"
echo.
echo Press any key to close this window.
pause >nul
exit /b %UFO50_CHS_EXIT%
