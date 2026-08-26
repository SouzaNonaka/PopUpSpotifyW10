using System.Windows;
using SpotifyMediaFlyout.Core.Audio;
using SpotifyMediaFlyout.Core.Media;
using SpotifyMediaFlyout.Core.State;
using SpotifyMediaFlyout.Infrastructure.Input;
using SpotifyMediaFlyout.Infrastructure.Tray;
using SpotifyMediaFlyout.Services;

namespace SpotifyMediaFlyout;

public partial class App : System.Windows.Application
{
    private FlyoutState? _state;
    private AudioController? _audioController;
    private MediaController? _mediaController;
    private OverlayService? _overlayService;
    private TrayService? _trayService;
    private GlobalMediaKeyHook? _globalMediaKeyHook;

    protected override void OnStartup(System.Windows.StartupEventArgs e)
    {
        base.OnStartup(e);

        _state = new FlyoutState();

        _audioController = new AudioController();
        _state.Volume = _audioController.GetVolume();
        _state.IsMuted = _audioController.GetMuteState();

        _audioController.VolumeChanged += (s, args) =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (_state != null && _overlayService != null)
                {
                    _state.Volume = args.Volume;
                    _state.IsMuted = args.IsMuted;
                    _overlayService.ShowVolume();
                }
            });
        };

        _mediaController = new MediaController();
        _mediaController.MediaChanged += (s, args) =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (_state != null)
                {
                    UpdateStateFromMedia(args.MediaInfo);
                }
            });
        };

        _mediaController.PlaybackChanged += (s, args) =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (_state != null)
                {
                    UpdateStateFromMedia(args.MediaInfo);
                }
            });
        };

        _overlayService = new OverlayService(_state);
        _overlayService.MuteToggled += (s, args) => _audioController?.ToggleMute();
        _overlayService.VolumeChangedByCard += (s, vol) => _audioController?.SetVolume(vol);

        _overlayService.PreviousClicked += async (s, args) =>
        {
            _overlayService.ShowMedia();
            if (_mediaController != null) await _mediaController.PreviousAsync();
        };
        _overlayService.PlayPauseClicked += async (s, args) =>
        {
            _overlayService.ShowMedia();
            if (_mediaController != null) await _mediaController.PlayPauseAsync();
        };
        _overlayService.NextClicked += async (s, args) =>
        {
            _overlayService.ShowMedia();
            if (_mediaController != null) await _mediaController.NextAsync();
        };

        _globalMediaKeyHook = new GlobalMediaKeyHook();
        _globalMediaKeyHook.MediaKeyPressed += (s, args) =>
        {
            Dispatcher.InvokeAsync(() =>
            {
                if (_overlayService != null && (_state?.IsSpotifyAvailable == true || _mediaController?.CurrentInfo.IsAvailable == true))
                {
                    _overlayService.ShowMedia();
                }
            });
        };

        _trayService = new TrayService(
            onShowTest: ShowTestFlyout,
            onExit: ExitApplication
        );
    }

    private void UpdateStateFromMedia(MediaSessionInfo info)
    {
        if (_state == null) return;

        _state.IsSpotifyAvailable = info.IsAvailable;
        _state.Title = info.Title;
        _state.Artist = info.Artist;
        _state.Album = info.Album;
        _state.Artwork = info.Artwork;
        _state.PlaybackState = info.PlaybackStatus;
        _state.CanPlayPause = info.CanPlayPause;
        _state.CanNext = info.CanNext;
        _state.CanPrevious = info.CanPrevious;
    }

    private void ShowTestFlyout()
    {
        if (_state == null || _overlayService == null) return;

        if (!_state.IsSpotifyAvailable || string.IsNullOrEmpty(_state.Title))
        {
            _state.IsSpotifyAvailable = true;
            _state.Title = "Kingdom";
            _state.Artist = "Archie";
            _state.Album = "Sample Album";
            _state.PlaybackState = global::Windows.Media.Control.GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;
            _state.CanPlayPause = true;
            _state.CanNext = true;
            _state.CanPrevious = true;
        }

        if (_audioController != null)
        {
            _state.Volume = _audioController.GetVolume();
            _state.IsMuted = _audioController.GetMuteState();
        }

        _overlayService.Show();
    }

    private void ExitApplication()
    {
        _globalMediaKeyHook?.Dispose();
        _trayService?.Dispose();
        _overlayService?.Dispose();
        _mediaController?.Dispose();
        _audioController?.Dispose();

        Shutdown();
    }

    protected override void OnExit(System.Windows.ExitEventArgs e)
    {
        _globalMediaKeyHook?.Dispose();
        _trayService?.Dispose();
        _overlayService?.Dispose();
        _mediaController?.Dispose();
        _audioController?.Dispose();

        base.OnExit(e);
    }
}
