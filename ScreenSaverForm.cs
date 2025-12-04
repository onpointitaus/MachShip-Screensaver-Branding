using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace BrandScreensaver
{
    public class ScreenSaverForm : Form
    {
        private readonly WebView2 webView;
        private readonly Label fallbackLabel;
        private bool isPreview;

        public ScreenSaverForm(bool preview = false)
        {
            isPreview = preview;
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = preview ? FormWindowState.Normal : FormWindowState.Maximized;
            this.BackColor = Color.Black;

            webView = new WebView2 { Dock = DockStyle.Fill, Visible = false };
            fallbackLabel = new Label { Dock = DockStyle.Fill, ForeColor = Color.White, TextAlign = ContentAlignment.MiddleCenter, Text = "MachShip Screensaver" };

            this.Controls.Add(webView);
            this.Controls.Add(fallbackLabel);

            this.Load += ScreenSaverForm_Load;
            this.MouseMove += ScreenSaverForm_MouseMove;
            this.KeyDown += ScreenSaverForm_KeyDown;
            this.FormClosing += ScreenSaverForm_FormClosing;
        }

        private void ScreenSaverForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            webView?.Dispose();
        }

        private void ScreenSaverForm_KeyDown(object sender, KeyEventArgs e)
        {
            Close();
        }

        private Point lastMouse;

        private void ScreenSaverForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPreview) return;
            if (lastMouse == Point.Empty)
            {
                lastMouse = e.Location; return;
            }
            if (Math.Abs(e.X - lastMouse.X) > 5 || Math.Abs(e.Y - lastMouse.Y) > 5)
            {
                Close();
            }
        }

        private async void ScreenSaverForm_Load(object sender, EventArgs e)
        {
            try
            {
                await InitializeWebViewAsync();
                var indexPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "content", "index.html");
                if (System.IO.File.Exists(indexPath))
                {
                    webView.CoreWebView2.Navigate(indexPath);
                    webView.Visible = true;
                    fallbackLabel.Visible = false;
                }
                else
                {
                    fallbackLabel.Text = "Content not found.";
                }
            }
            catch (Exception ex)
            {
                fallbackLabel.Text = "WebView2 unavailable. " + ex.Message;
            }
        }

        private async Task InitializeWebViewAsync()
        {
            var env = await CoreWebView2Environment.CreateAsync();
            await webView.EnsureCoreWebView2Async(env);
        }
    }
}
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace BrandScreensaver
{
    public class ScreenSaverForm : Form
    {
        private readonly Timer _closeTimer = new Timer();
        private Point _mousePoint;
        private WebView2? _webView;

        public ScreenSaverForm()
        {
            InitializeFullScreen();
        }

        public ScreenSaverForm(IntPtr previewParent)
        {
            InitializePreview(previewParent);
        }

        private void InitializeFullScreen()
        {
            FormBorderStyle = FormBorderStyle.None;
            BackColor = Color.Black;
            ShowInTaskbar = false;
            WindowState = FormWindowState.Normal;
            Bounds = Screen.PrimaryScreen.Bounds;
            TopMost = true;
            DoubleBuffered = true;

            _webView = new WebView2 { Dock = DockStyle.Fill };
            Controls.Add(_webView);

            Load += async (_, __) => await InitializeWebViewAsync();

            _closeTimer.Interval = 50;
            _closeTimer.Start();

            KeyDown += (_, __) => Close();
            MouseMove += OnMouseMoveClose;
            MouseClick += (_, __) => Close();
        }

        private void InitializePreview(IntPtr previewParent)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            SetParent(Handle, previewParent);
            int style = GetWindowLong(Handle, GWL_STYLE);
            style = style | WS_CHILD;
            SetWindowLong(Handle, GWL_STYLE, style);

            _webView = new WebView2 { Dock = DockStyle.Fill };
            Controls.Add(_webView);

            Load += async (_, __) => await InitializeWebViewAsync();

            _closeTimer.Interval = 50;
            _closeTimer.Start();
        }

        private async Task InitializeWebViewAsync()
        {
            try
            {
                var env = await CoreWebView2Environment.CreateAsync();
                if (_webView is null) return;
                await _webView.EnsureCoreWebView2Async(env);

                string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "content", "index.html");
                if (File.Exists(htmlPath))
                {
                    _webView.CoreWebView2.Navigate(new Uri(htmlPath).AbsoluteUri);
                }
                else
                {
                    _webView.CoreWebView2.NavigateToString("<html><body style='background:#000;color:#fff;font-family:Segoe UI'><h2>Brand content missing</h2></body></html>");
                }

                _webView.CoreWebView2.Settings.AreDevToolsEnabled = false;
                _webView.CoreWebView2.Settings.IsScriptEnabled = true;
            }
            catch (Exception)
            {
                Controls.Clear();
                var lbl = new Label
                {
                    Dock = DockStyle.Fill,
                    Text = "MachShip Screensaver",
                    ForeColor = Color.White,
                    BackColor = Color.Black,
                    Font = new Font("Segoe UI", 48, FontStyle.Bold),
                    TextAlign = ContentAlignment.MiddleCenter
                };
                Controls.Add(lbl);
            }
        }

        private void OnMouseMoveClose(object? sender, MouseEventArgs e)
        {
            if (_mousePoint == Point.Empty) _mousePoint = e.Location;
            else if (Math.Abs(e.X - _mousePoint.X) > 5 || Math.Abs(e.Y - _mousePoint.Y) > 5) Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _closeTimer.Dispose();
                _webView?.Dispose();
            }
            base.Dispose(disposing);
        }

        private const int GWL_STYLE = -16;
        private const int WS_CHILD = 0x40000000;

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
