@echo off
setlocal
title UFO50-CHS Installer
powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File "%~dp0installer\Install-UFO50-CHS.ps1"
set "UFO50_CHS_EXIT=%ERRORLEVEL%"
echo.
if not "%UFO50_CHS_EXIT%"=="0" echo Installation failed. Review the error message above.
echo Press any key to close this window.
pause >nul
exit /b %UFO50_CHS_EXIT%
