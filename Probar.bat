@echo off
rem Abre GirarPantalla mostrando la consola, para ver errores si algo falla.
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0GirarPantalla.ps1"
pause
