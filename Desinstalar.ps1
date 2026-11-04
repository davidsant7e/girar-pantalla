# Quita el arranque automático de GirarPantalla. Antes ciérralo: clic derecho en el ícono → Salir.

$acceso = Join-Path ([Environment]::GetFolderPath("Startup")) "GirarPantalla.lnk"
if (Test-Path $acceso) {
    Remove-Item $acceso
    Write-Host "Listo: GirarPantalla ya no arranca con Windows."
} else {
    Write-Host "No estaba instalado en el inicio de Windows."
}
