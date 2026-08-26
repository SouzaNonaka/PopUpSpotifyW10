using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using SpotifyMediaFlyout.Services;

namespace SpotifyMediaFlyout.Infrastructure.Tray;

public sealed class TrayService : IDisposable
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ToolStripMenuItem _startupMenuItem;
    private readonly Action _onShowTest;
    private readonly Action _onExit;
    private bool _disposed;

    public TrayService(Action onShowTest, Action onExit)
    {
        _onShowTest = onShowTest;
        _onExit = onExit;

        var contextMenu = new ContextMenuStrip();

        var titleItem = new ToolStripMenuItem("Spotify Media Flyout")
        {
            Enabled = false,
            Font = new Font(Control.DefaultFont, FontStyle.Bold)
        };
        contextMenu.Items.Add(titleItem);
        contextMenu.Items.Add(new ToolStripSeparator());

        _startupMenuItem = new ToolStripMenuItem("Iniciar com Windows")
        {
            CheckOnClick = true,
            Checked = StartupService.IsStartupEnabled()
        };
        _startupMenuItem.Click += OnStartupMenuItemClick;
        contextMenu.Items.Add(_startupMenuItem);

        contextMenu.Items.Add(new ToolStripSeparator());

        var testItem = new ToolStripMenuItem("Mostrar teste");
        testItem.Click += (s, e) => _onShowTest();
        contextMenu.Items.Add(testItem);

        var exitItem = new ToolStripMenuItem("Sair");
        exitItem.Click += (s, e) => _onExit();
        contextMenu.Items.Add(exitItem);

        _notifyIcon = new NotifyIcon
        {
            Text = "Spotify Media Flyout",
            Icon = CreateTrayIcon(),
            ContextMenuStrip = contextMenu,
            Visible = true
        };

        _notifyIcon.DoubleClick += (s, e) => _onShowTest();
    }

    private void OnStartupMenuItemClick(object? sender, EventArgs e)
    {
        bool isEnabled = _startupMenuItem.Checked;
        StartupService.SetStartup(isEnabled);
    }

    private static Icon CreateTrayIcon()
    {
        using var bitmap = new Bitmap(32, 32);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;

            using var brush = new SolidBrush(Color.FromArgb(29, 185, 84));
            g.FillEllipse(brush, 2, 2, 28, 28);

            using var whiteBrush = new SolidBrush(Color.White);
            using var pen = new Pen(Color.White, 2f);

            Point[] cone = {
                new Point(8, 13),
                new Point(12, 13),
                new Point(17, 9),
                new Point(17, 23),
                new Point(12, 19),
                new Point(8, 19)
            };
            g.FillPolygon(whiteBrush, cone);

            g.DrawArc(pen, 16, 12, 8, 8, -60, 120);
            g.DrawArc(pen, 19, 9, 14, 14, -60, 120);
        }

        IntPtr hIcon = bitmap.GetHicon();
        return Icon.FromHandle(hIcon);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
