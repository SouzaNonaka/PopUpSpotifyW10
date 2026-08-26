using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using SpotifyMediaFlyout.Config;
using SpotifyMediaFlyout.Core.State;

namespace SpotifyMediaFlyout.UI;

public partial class OverlayWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_TOPMOST = 0x00000008;

    private const int WM_MOUSEACTIVATE = 0x0021;
    private const int MA_NOACTIVATE = 3;

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "GetWindowLong", SetLastError = true)]
    private static extern int GetWindowLong32(IntPtr hWnd, int nIndex);

    private static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
    {
        return IntPtr.Size == 8 ? GetWindowLongPtr64(hWnd, nIndex) : new IntPtr(GetWindowLong32(hWnd, nIndex));
    }

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", SetLastError = true)]
    private static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

    [DllImport("user32.dll", EntryPoint = "SetWindowLong", SetLastError = true)]
    private static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

    private static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
    {
        return IntPtr.Size == 8 ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong) : new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
    }

    public event EventHandler? MouseEnteredWindow;
    public event EventHandler? MouseLeftWindow;

    public event EventHandler? MuteToggled;
    public event EventHandler<int>? VolumeChangedByCard;
    public event EventHandler? PreviousClicked;
    public event EventHandler? PlayPauseClicked;
    public event EventHandler? NextClicked;

    private Storyboard? _currentAnimation;

    public OverlayWindow(FlyoutState state)
    {
        InitializeComponent();
        DataContext = state;

        VolumeCardControl.DataContext = state;
        MediaCardControl.DataContext = state;

        VolumeCardControl.MuteToggled += (s, e) => MuteToggled?.Invoke(this, e);
        VolumeCardControl.VolumeChangedByCard += (s, vol) => VolumeChangedByCard?.Invoke(this, vol);
        MediaCardControl.PreviousClicked += (s, e) => PreviousClicked?.Invoke(this, e);
        MediaCardControl.PlayPauseClicked += (s, e) => PlayPauseClicked?.Invoke(this, e);
        MediaCardControl.NextClicked += (s, e) => NextClicked?.Invoke(this, e);

        MouseEnter += (s, e) => MouseEnteredWindow?.Invoke(this, EventArgs.Empty);
        MouseLeave += (s, e) => MouseLeftWindow?.Invoke(this, EventArgs.Empty);

        SourceInitialized += OnSourceInitialized;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        var helper = new WindowInteropHelper(this);
        var hWnd = helper.Handle;

        long exStyle = GetWindowLongPtr(hWnd, GWL_EXSTYLE).ToInt64();
        exStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
        SetWindowLongPtr(hWnd, GWL_EXSTYLE, new IntPtr(exStyle));

        var source = HwndSource.FromHwnd(hWnd);
        source?.AddHook(WndProc);

        UpdatePosition();
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == WM_MOUSEACTIVATE)
        {
            handled = true;
            return new IntPtr(MA_NOACTIVATE);
        }
        return IntPtr.Zero;
    }

    public void UpdatePosition()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Left + AppSettings.FlyoutMarginLeft - 10;
        Top = workArea.Top + AppSettings.FlyoutMarginTop - 10;
    }

    public void FadeIn(Action? onComplete = null)
    {
        UpdatePosition();
        Show();

        _currentAnimation?.Stop();

        var animation = new DoubleAnimation
        {
            To = 1.0,
            Duration = TimeSpan.FromMilliseconds(AppSettings.AnimationDurationMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };

        var sb = new Storyboard();
        sb.Children.Add(animation);
        Storyboard.SetTarget(animation, this);
        Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));

        if (onComplete != null)
        {
            sb.Completed += (s, e) => onComplete();
        }

        _currentAnimation = sb;
        sb.Begin();
    }

    public void FadeOut(Action? onComplete = null)
    {
        _currentAnimation?.Stop();

        var animation = new DoubleAnimation
        {
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(AppSettings.AnimationDurationMs),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
        };

        var sb = new Storyboard();
        sb.Children.Add(animation);
        Storyboard.SetTarget(animation, this);
        Storyboard.SetTargetProperty(animation, new PropertyPath(OpacityProperty));

        sb.Completed += (s, e) =>
        {
            Hide();
            onComplete?.Invoke();
        };

        _currentAnimation = sb;
        sb.Begin();
    }
}
