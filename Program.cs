using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BrandScreensaver
{
    static class Program
    {
        [DllImport("user32.dll")]
        private static extern bool SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [STAThread]
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mode = args.Length == 0 ? "/s" : args[0].ToLowerInvariant();

            if (mode == "/s")
            {
                var form = new ScreenSaverForm();
                Application.Run(form);
            }
            else if (mode == "/p" && args.Length >= 2 && int.TryParse(args[1], out var hwndInt))
            {
                var previewHandle = new IntPtr(hwndInt);
                var form = new ScreenSaverForm(true);
                SetParent(form.Handle, previewHandle);
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Bounds = new System.Drawing.Rectangle(0, 0, 200, 200);
                form.Show();
                Application.Run();
            }
            else if (mode == "/c")
            {
                MessageBox.Show("No configurable options.", "MachShip Screensaver", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // unknown args - show the preview fullscreen
                var form = new ScreenSaverForm();
                Application.Run(form);
            }
        }
    }
}
using System;
using System.Windows.Forms;

namespace BrandScreensaver
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            string mode = args.Length > 0 ? args[0].ToLowerInvariant() : "/s";

            if (mode == "/c")
            {
                MessageBox.Show("MachShip Screensaver settings (none).", "Screensaver", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (mode == "/p" && args.Length > 1)
            {
                if (long.TryParse(args[1], out long previewWnd))
                {
                    IntPtr parent = new IntPtr(previewWnd);
                    var preview = new ScreenSaverForm(parent);
                    Application.Run(preview);
                    return;
                }
            }

            var form = new ScreenSaverForm();
            Application.Run(form);
        }
    }
}
