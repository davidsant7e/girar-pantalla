# Crea un acceso directo en la carpeta "Inicio" de Windows para que GirarPantalla
# arranque solo al iniciar sesión, y lo abre de una vez.
# Para quitarlo: ejecuta Desinstalar.ps1.

$ErrorActionPreference = "Stop"

$script = Join-Path $PSScriptRoot "GirarPantalla.ps1"
$inicio = [Environment]::GetFolderPath("Startup")
$acceso = Join-Path $inicio "GirarPantalla.lnk"
$argumentos = "-NoProfile -WindowStyle Hidden -ExecutionPolicy Bypass -File `"$script`""

$shell = New-Object -ComObject WScript.Shell
$lnk = $shell.CreateShortcut($acceso)
$lnk.TargetPath = "$env:SystemRoot\System32\WindowsPowerShell\v1.0\powershell.exe"
$lnk.Arguments = $argumentos
$lnk.WorkingDirectory = $PSScriptRoot
$lnk.WindowStyle = 7  # minimizada
$lnk.Description = "Girar la pantalla con Ctrl+Alt+Win + flechas"
$lnk.IconLocation = "$env:SystemRoot\System32\shell32.dll,15"
$lnk.Save()

Write-Host "Acceso directo creado en: $acceso"
Write-Host "GirarPantalla arrancará solo cada vez que inicies sesión."

Start-Process -FilePath $lnk.TargetPath -ArgumentList $argumentos -WindowStyle Hidden
Write-Host "Abierto ahora: busca el ícono junto al reloj (puede estar en la flecha ^)."
