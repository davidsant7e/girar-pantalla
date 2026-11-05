# GirarPantalla: gira la pantalla con atajos de teclado.
#
#   Ctrl+Alt+Win + ↑   normal
#   Ctrl+Alt+Win + ←   vertical (la parte de arriba de la imagen queda a la izquierda)
#   Ctrl+Alt+Win + →   vertical (la parte de arriba de la imagen queda a la derecha)
#   Ctrl+Alt+Win + ↓   de cabeza
#
# Gira la pantalla donde está el mouse. Queda un ícono junto al reloj (clic derecho → Salir).
# Para que arranque con Windows: ejecuta Instalar.ps1.

# ===================== AJUSTES =====================
# Teclas que se mantienen presionadas junto con las flechas. Puedes combinar:
# Ctrl, Alt, Shift, Win (separadas por +).
# Se evita Ctrl+Alt sola porque Android Studio la usa para "Back/Forward" (Ctrl+Alt+← / →),
# y Win+Ctrl sola porque Windows la usa para cambiar de escritorio virtual.
$Atajo = "Ctrl+Alt+Win"

# Si ← y → giran al revés de lo que esperas, cambia esto a $true.
$InvertirLados = $false
# ===================================================

$ErrorActionPreference = "Stop"

$mods = 0
foreach ($parte in $Atajo.Split("+")) {
    switch ($parte.Trim().ToLower()) {
        "ctrl"    { $mods = $mods -bor 0x2 }
        "control" { $mods = $mods -bor 0x2 }
        "alt"     { $mods = $mods -bor 0x1 }
        "shift"   { $mods = $mods -bor 0x4 }
        "win"     { $mods = $mods -bor 0x8 }
        "windows" { $mods = $mods -bor 0x8 }
        default   { throw "Tecla desconocida en `$Atajo: '$parte'. Usa Ctrl, Alt, Shift o Win." }
    }
}
if ($mods -eq 0) { throw "`$Atajo necesita al menos una tecla (Ctrl, Alt, Shift o Win)." }

# Compila el código C# que está al lado (lo hace Windows; no hay que instalar nada).
$codigo = Get-Content -Raw -Encoding UTF8 -Path (Join-Path $PSScriptRoot "GirarPantalla.cs")
Add-Type -TypeDefinition $codigo -ReferencedAssemblies "System.Windows.Forms", "System.Drawing"

$icono = Join-Path $PSScriptRoot "assets\girar-pantalla.ico"
[GirarPantalla.App]::Run($mods, $Atajo, $InvertirLados, $icono)
