# GirarPantalla

<p align="center"><img src="docs/carta.svg" alt="X · La Rueda de la Fortuna" width="180"></p>


Gira la pantalla de Windows con un atajo de teclado.

| Atajo | Resultado |
|---|---|
| **Ctrl + Alt + Win + ↑** | Normal (horizontal) |
| **Ctrl + Alt + Win + ←** | Vertical: la parte de arriba de la imagen queda a la izquierda |
| **Ctrl + Alt + Win + →** | Vertical: la parte de arriba de la imagen queda a la derecha |
| **Ctrl + Alt + Win + ↓** | De cabeza |

- Gira **la pantalla donde está el mouse** (sirve si conectas el TV u otro monitor).
- Queda el ícono de **X · La Rueda de la Fortuna** junto al reloj; con clic derecho tienes el mismo menú y **Salir**.
- Si algo se ve mal, **Ctrl + Alt + Win + ↑** siempre la devuelve a la normal.
- No instala nada: usa PowerShell y .NET, que ya vienen con Windows 11.

## Instalar

1. Descomprime la carpeta en `C:\Git\girar-pantalla`.
2. Doble clic en **`Probar.bat`**: se abre con una consola para ver si hay errores. Prueba los atajos.
3. Si todo funciona, cierra la consola y haz doble clic en **`Instalar.bat`**: desde ahí arranca solo con Windows.

Para quitarlo: clic derecho en el ícono → **Salir**, y doble clic en **`Desinstalar.bat`**.

## Cambiar el atajo

Abre `GirarPantalla.ps1` con el Bloc de notas y cambia la línea:

```powershell
$Atajo = "Ctrl+Alt+Win"
```

Puedes combinar `Ctrl`, `Alt`, `Shift` y `Win`. Combinaciones que conviene evitar:

- `Ctrl+Alt`: Android Studio la usa para navegar atrás/adelante (Ctrl+Alt+← / →).
- `Win+Ctrl`: Windows la usa para cambiar de escritorio virtual.
- `Win` sola o `Win+Shift`: Windows las usa para acomodar ventanas y moverlas entre monitores.

Si ← y → giran al revés de lo que esperas, pon `$InvertirLados = $true`.

Guarda, sal de GirarPantalla (clic derecho → Salir) y ábrelo otra vez.

## Archivos

| Archivo | Qué hace |
|---|---|
| `GirarPantalla.cs` | El programa (C#): gira la pantalla con `ChangeDisplaySettingsEx`, registra los atajos con `RegisterHotKey` y muestra el ícono. |
| `assets/girar-pantalla.ico` | Ícono del arcano X · La Rueda de la Fortuna (bandeja y acceso directo). |
| `GirarPantalla.ps1` | Ajustes (atajo) y arranque: compila el `.cs` en memoria con `Add-Type` y lo ejecuta. |
| `Probar.bat` | Lo abre con consola, para ver errores. |
| `Instalar.bat` / `.ps1` | Acceso directo en la carpeta Inicio de Windows (`shell:startup`) y lo abre. |
| `Desinstalar.bat` / `.ps1` | Quita ese acceso directo. |

## Si algo falla

- **"Otra app ya usa…"**: ese atajo está tomado; cambia `$Atajo`.
- **"Esta pantalla no permite esa orientación"**: el controlador de video no deja girar; actualiza el controlador Intel desde Windows Update.
- **Al iniciar Windows aparece un instante una ventana azul**: es PowerShell arrancando oculto; es normal.
- **Ya está abierto**: solo puede haber uno; busca el ícono en la flecha **^** junto al reloj.
