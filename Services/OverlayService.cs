using System.Windows.Threading;
using SpotifyMediaFlyout.Config;
using SpotifyMediaFlyout.Core.State;
using SpotifyMediaFlyout.UI;

namespace SpotifyMediaFlyout.Services;

public sealed class OverlayService : IDisposable
{
    private readonly OverlayWindow _window;
    private readonly DispatcherTimer _hideTimer;
    private bool _isMouseOver;
    private bool _isVisible;
    private bool _disposed;

    public event EventHandler? MuteToggled;
    public event EventHandler<int>? VolumeChangedByCard;
    public event EventHandler? PreviousClicked;
    public event EventHandler? PlayPauseClicked;
    public event EventHandler? NextClicked;

    public OverlayService(FlyoutState state)
    {
        _window = new OverlayWindow(state);

        _hideTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(AppSettings.DisplayDurationMs)
        };
        _hideTimer.Tick += OnHideTimerTick;

        _window.MouseEnteredWindow += OnMouseEnteredWindow;
        _window.MouseLeftWindow += OnMouseLeftWindow;

        _window.MuteToggled += (s, e) => MuteToggled?.Invoke(this, e);
        _window.VolumeChangedByCard += (s, vol) => VolumeChangedByCard?.Invoke(this, vol);
        _window.PreviousClicked += (s, e) => PreviousClicked?.Invoke(this, e);
        _window.PlayPauseClicked += (s, e) => PlayPauseClicked?.Invoke(this, e);
        _window.NextClicked += (s, e) => NextClicked?.Invoke(this, e);
    }

    private void OnMouseEnteredWindow(object? sender, EventArgs e)
    {
        _isMouseOver = true;
        _hideTimer.Stop();
    }

    private void OnMouseLeftWindow(object? sender, EventArgs e)
    {
        _isMouseOver = false;
        if (_isVisible)
        {
            ResetTimer();
        }
    }

    public void Show()
    {
        _window.Dispatcher.Invoke(() =>
        {
            _isVisible = true;
            _window.FadeIn();
            ResetTimer();
        });
    }

    public void ShowVolume()
    {
        Show();
    }

    public void ShowMedia()
    {
        Show();
    }

    public void Hide()
    {
        _window.Dispatcher.Invoke(() =>
        {
            _hideTimer.Stop();
            _isVisible = false;
            _window.FadeOut();
        });
    }

    public void ResetTimer()
    {
        _window.Dispatcher.Invoke(() =>
        {
            _hideTimer.Stop();
            if (!_isMouseOver)
            {
                _hideTimer.Start();
            }
        });
    }

    private void OnHideTimerTick(object? sender, EventArgs e)
    {
        _hideTimer.Stop();
        if (!_isMouseOver && _isVisible)
        {
            _isVisible = false;
            _window.FadeOut();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _hideTimer.Stop();
        _window.Dispatcher.Invoke(() =>
        {
            _window.Close();
        });
    }
}
