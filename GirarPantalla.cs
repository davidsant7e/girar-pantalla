// GirarPantalla: gira la pantalla de Windows con atajos de teclado.
// Lo compila y carga GirarPantalla.ps1 con Add-Type (no hay que instalar nada).
// Escrito en C# 5 porque es la versión que trae Windows PowerShell 5.1.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace GirarPantalla
{
    /// <summary>Orientaciones de Windows (valores de DEVMODE.dmDisplayOrientation).</summary>
    public enum Orientacion
    {
        Normal = 0,          // horizontal
        Vertical = 1,        // la parte de arriba de la imagen queda a la izquierda
        Invertida = 2,       // de cabeza (180°)
        VerticalInvertida = 3 // la parte de arriba de la imagen queda a la derecha
    }

    /// <summary>Lectura y cambio de la orientación de una pantalla con la API de Windows.</summary>
    public static class Pantalla
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct DEVMODE
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;
            public int dmPositionX;
            public int dmPositionY;
            public int dmDisplayOrientation;
            public int dmDisplayFixedOutput;
            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        private const int ENUM_CURRENT_SETTINGS = -1;
        private const int DM_DISPLAYORIENTATION = 0x00000080;
        private const int DM_PELSWIDTH = 0x00080000;
        private const int DM_PELSHEIGHT = 0x00100000;
        private const int CDS_UPDATEREGISTRY = 0x00000001;
        private const int DISP_CHANGE_SUCCESSFUL = 0;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int ChangeDisplaySettingsEx(
            string deviceName, ref DEVMODE devMode, IntPtr hwnd, int flags, IntPtr lParam);

        /// <summary>Nombre de la pantalla donde está el mouse (por ejemplo \\.\DISPLAY1).</summary>
        public static string PantallaDelMouse()
        {
            return Screen.FromPoint(Cursor.Position).DeviceName;
        }

        public static Orientacion Actual(string dispositivo)
        {
            DEVMODE dm = Nuevo();
            if (!EnumDisplaySettings(dispositivo, ENUM_CURRENT_SETTINGS, ref dm))
            {
                throw new InvalidOperationException("No se pudo leer la configuración de " + dispositivo + ".");
            }
            return (Orientacion)dm.dmDisplayOrientation;
        }

        /// <summary>
        /// Cambia la orientación. Al pasar de horizontal a vertical (o al revés) intercambia
        /// el ancho y el alto, como hace Configuración → Pantalla. El cambio queda guardado.
        /// </summary>
        public static void Cambiar(string dispositivo, Orientacion nueva)
        {
            DEVMODE dm = Nuevo();
            if (!EnumDisplaySettings(dispositivo, ENUM_CURRENT_SETTINGS, ref dm))
            {
                throw new InvalidOperationException("No se pudo leer la configuración de " + dispositivo + ".");
            }

            int anterior = dm.dmDisplayOrientation;
            int destino = (int)nueva;
            if (anterior == destino) return;

            if ((anterior % 2) != (destino % 2))
            {
                int ancho = dm.dmPelsWidth;
                dm.dmPelsWidth = dm.dmPelsHeight;
                dm.dmPelsHeight = ancho;
            }
            dm.dmDisplayOrientation = destino;
            dm.dmFields = DM_DISPLAYORIENTATION | DM_PELSWIDTH | DM_PELSHEIGHT;

            int resultado = ChangeDisplaySettingsEx(dispositivo, ref dm, IntPtr.Zero, CDS_UPDATEREGISTRY, IntPtr.Zero);
            if (resultado != DISP_CHANGE_SUCCESSFUL)
            {
                throw new InvalidOperationException(MensajeDeError(resultado));
            }
        }

        private static DEVMODE Nuevo()
        {
            DEVMODE dm = new DEVMODE();
            dm.dmDeviceName = new string('\0', 32);
            dm.dmFormName = new string('\0', 32);
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            return dm;
        }

        private static string MensajeDeError(int codigo)
        {
            switch (codigo)
            {
                case 1: return "Windows pide reiniciar para aplicar el giro.";
                case -1: return "El controlador de video no aceptó el giro.";
                case -2: return "Esta pantalla no permite esa orientación (revisa el controlador de video).";
                case -3: return "No se pudo guardar la configuración.";
                case -4: return "Configuración inválida.";
                case -5: return "Parámetros inválidos.";
                default: return "Windows devolvió el código " + codigo + ".";
            }
        }
    }

    /// <summary>Ventana invisible que recibe los atajos de teclado registrados (WM_HOTKEY).</summary>
    internal class VentanaAtajos : NativeWindow, IDisposable
    {
        private const int WM_HOTKEY = 0x0312;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int modifiers, int vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly Dictionary<int, Action> acciones = new Dictionary<int, Action>();
        private int siguienteId = 1;

        public VentanaAtajos()
        {
            CreateHandle(new CreateParams());
        }

        /// <summary>Registra un atajo. Devuelve false si otra app ya lo tiene.</summary>
        public bool Registrar(int modificadores, Keys tecla, Action accion)
        {
            int id = siguienteId++;
            if (!RegisterHotKey(Handle, id, modificadores, (int)tecla)) return false;
            acciones[id] = accion;
            return true;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                Action accion;
                if (acciones.TryGetValue(m.WParam.ToInt32(), out accion)) accion();
                return;
            }
            base.WndProc(ref m);
        }

        public void Dispose()
        {
            foreach (int id in acciones.Keys) UnregisterHotKey(Handle, id);
            acciones.Clear();
            DestroyHandle();
        }
    }

    /// <summary>La aplicación: ícono junto al reloj, menú y atajos.</summary>
    public static class App
    {
        public const int MOD_ALT = 0x1;
        public const int MOD_CONTROL = 0x2;
        public const int MOD_SHIFT = 0x4;
        public const int MOD_WIN = 0x8;
        private const int MOD_NOREPEAT = 0x4000;

        private static NotifyIcon icono;

        /// <param name="modificadores">Suma de MOD_* (por ejemplo MOD_CONTROL | MOD_ALT | MOD_WIN).</param>
        /// <param name="textoAtajo">Cómo se muestra el atajo, por ejemplo "Ctrl+Alt+Win".</param>
        /// <param name="invertirLados">true si las flechas izquierda/derecha giran al revés de lo esperado.</param>
        public static void Run(int modificadores, string textoAtajo, bool invertirLados)
        {
            bool esNueva;
            using (Mutex unico = new Mutex(true, "GirarPantalla_dseu1", out esNueva))
            {
                if (!esNueva)
                {
                    MessageBox.Show("GirarPantalla ya está abierto (ícono junto al reloj).", "GirarPantalla");
                    return;
                }

                Application.EnableVisualStyles();

                Orientacion izquierda = invertirLados ? Orientacion.VerticalInvertida : Orientacion.Vertical;
                Orientacion derecha = invertirLados ? Orientacion.Vertical : Orientacion.VerticalInvertida;

                using (VentanaAtajos atajos = new VentanaAtajos())
                using (icono = new NotifyIcon())
                {
                    icono.Icon = DibujarIcono();
                    icono.Text = "GirarPantalla (" + textoAtajo + " + flechas)";
                    icono.ContextMenuStrip = CrearMenu(textoAtajo, izquierda, derecha);
                    icono.Visible = true;

                    int mods = modificadores | MOD_NOREPEAT;
                    List<string> fallidos = new List<string>();
                    if (!atajos.Registrar(mods, Keys.Up, delegate { Girar(Orientacion.Normal); })) fallidos.Add("↑");
                    if (!atajos.Registrar(mods, Keys.Down, delegate { Girar(Orientacion.Invertida); })) fallidos.Add("↓");
                    if (!atajos.Registrar(mods, Keys.Left, delegate { Girar(izquierda); })) fallidos.Add("←");
                    if (!atajos.Registrar(mods, Keys.Right, delegate { Girar(derecha); })) fallidos.Add("→");

                    if (fallidos.Count > 0)
                    {
                        Avisar("Otra app ya usa " + textoAtajo + " + " + string.Join(" ", fallidos.ToArray()) +
                               ". Cambia el atajo en GirarPantalla.ps1.", ToolTipIcon.Warning);
                    }
                    else
                    {
                        Avisar("Listo: " + textoAtajo + " + flechas gira la pantalla donde está el mouse. " +
                               textoAtajo + " + ↑ la deja normal.", ToolTipIcon.Info);
                    }

                    Application.Run();
                    icono.Visible = false;
                }
            }
        }

        private static ContextMenuStrip CrearMenu(string atajo, Orientacion izquierda, Orientacion derecha)
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Items.Add("Normal\t" + atajo + "+↑", null, delegate { Girar(Orientacion.Normal); });
            menu.Items.Add("Vertical (arriba a la izquierda)\t" + atajo + "+←", null, delegate { Girar(izquierda); });
            menu.Items.Add("Vertical (arriba a la derecha)\t" + atajo + "+→", null, delegate { Girar(derecha); });
            menu.Items.Add("De cabeza\t" + atajo + "+↓", null, delegate { Girar(Orientacion.Invertida); });
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("Salir", null, delegate { Application.Exit(); });
            return menu;
        }

        private static void Girar(Orientacion orientacion)
        {
            string pantalla = Pantalla.PantallaDelMouse();
            try
            {
                Pantalla.Cambiar(pantalla, orientacion);
            }
            catch (Exception e)
            {
                Avisar(e.Message, ToolTipIcon.Error);
            }
        }

        private static void Avisar(string texto, ToolTipIcon tipo)
        {
            if (icono != null) icono.ShowBalloonTip(4000, "GirarPantalla", texto, tipo);
        }

        /// <summary>Ícono de 16×16: un monitor con una flecha de giro.</summary>
        private static Icon DibujarIcono()
        {
            using (Bitmap bmp = new Bitmap(16, 16))
            using (Graphics g = Graphics.FromImage(bmp))
            using (Pen borde = new Pen(Color.White, 2))
            using (Pen flecha = new Pen(Color.FromArgb(124, 242, 200), 2))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Color.Transparent);
                g.FillRectangle(new SolidBrush(Color.FromArgb(15, 95, 90)), 1, 2, 14, 10);
                g.DrawRectangle(borde, 1, 2, 14, 10);
                g.DrawLine(borde, 8, 12, 8, 15);
                g.DrawArc(flecha, 4, 4, 8, 6, 200, 280);
                return Icon.FromHandle(bmp.GetHicon());
            }
        }
    }
}
