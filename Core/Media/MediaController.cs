using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Media.Imaging;
using Windows.Media.Control;
using Windows.Storage.Streams;

namespace SpotifyMediaFlyout.Core.Media;

public sealed class MediaController : IDisposable
{
    private GlobalSystemMediaTransportControlsSessionManager? _sessionManager;
    private GlobalSystemMediaTransportControlsSession? _currentSession;
    private readonly object _lock = new();
    private bool _disposed;

    public event EventHandler<MediaChangedEventArgs>? MediaChanged;
    public event EventHandler<MediaChangedEventArgs>? PlaybackChanged;

    public MediaSessionInfo CurrentInfo { get; private set; } = new();

    public MediaController()
    {
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            _sessionManager = await GlobalSystemMediaTransportControlsSessionManager.RequestAsync();
            if (_sessionManager != null)
            {
                _sessionManager.CurrentSessionChanged += OnCurrentSessionChanged;
                _sessionManager.SessionsChanged += OnSessionsChanged;
                await RefreshCurrentSessionAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MediaController] GSMTC init failed: {ex.Message}");
        }
    }

    private async void OnCurrentSessionChanged(GlobalSystemMediaTransportControlsSessionManager sender, CurrentSessionChangedEventArgs args)
    {
        await RefreshCurrentSessionAsync();
    }

    private async void OnSessionsChanged(GlobalSystemMediaTransportControlsSessionManager sender, SessionsChangedEventArgs args)
    {
        await RefreshCurrentSessionAsync();
    }

    private async Task RefreshCurrentSessionAsync()
    {
        if (_sessionManager == null) return;

        GlobalSystemMediaTransportControlsSession? targetSession = null;
        try
        {
            var sessions = _sessionManager.GetSessions();
            targetSession = sessions.FirstOrDefault(s => IsSpotify(s.SourceAppUserModelId))
                         ?? _sessionManager.GetCurrentSession();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MediaController] GetSessions error: {ex.Message}");
        }

        lock (_lock)
        {
            if (_currentSession != null)
            {
                _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
            }

            _currentSession = targetSession;

            if (_currentSession != null)
            {
                _currentSession.MediaPropertiesChanged += OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged += OnPlaybackInfoChanged;
            }
        }

        await UpdateAllMediaInfoAsync();
    }

    private static bool IsSpotify(string appId)
    {
        return !string.IsNullOrEmpty(appId) &&
               appId.Contains("Spotify", StringComparison.OrdinalIgnoreCase);
    }

    private async void OnMediaPropertiesChanged(GlobalSystemMediaTransportControlsSession sender, MediaPropertiesChangedEventArgs args)
    {
        await UpdateAllMediaInfoAsync(isPlaybackOnly: false);
    }

    private async void OnPlaybackInfoChanged(GlobalSystemMediaTransportControlsSession sender, PlaybackInfoChangedEventArgs args)
    {
        await UpdateAllMediaInfoAsync(isPlaybackOnly: true);
    }

    public async Task UpdateAllMediaInfoAsync(bool isPlaybackOnly = false)
    {
        var session = _currentSession;
        if (session == null || !IsSpotify(session.SourceAppUserModelId))
        {
            var emptyInfo = new MediaSessionInfo { IsAvailable = false };
            CurrentInfo = emptyInfo;
            if (isPlaybackOnly)
                PlaybackChanged?.Invoke(this, new MediaChangedEventArgs(emptyInfo));
            else
                MediaChanged?.Invoke(this, new MediaChangedEventArgs(emptyInfo));
            return;
        }

        var newInfo = new MediaSessionInfo
        {
            IsAvailable = true
        };

        try
        {
            var playbackInfo = session.GetPlaybackInfo();
            if (playbackInfo != null)
            {
                newInfo.PlaybackStatus = playbackInfo.PlaybackStatus;
                var controls = playbackInfo.Controls;
                if (controls != null)
                {
                    newInfo.CanPlayPause = controls.IsPlayPauseToggleEnabled || controls.IsPlayEnabled || controls.IsPauseEnabled;
                    newInfo.CanNext = controls.IsNextEnabled;
                    newInfo.CanPrevious = controls.IsPreviousEnabled;
                }
            }

            var mediaProps = await session.TryGetMediaPropertiesAsync();
            if (mediaProps != null)
            {
                newInfo.Title = mediaProps.Title ?? string.Empty;
                newInfo.Artist = mediaProps.Artist ?? string.Empty;
                newInfo.Album = mediaProps.AlbumTitle ?? string.Empty;

                if (mediaProps.Thumbnail != null)
                {
                    newInfo.Artwork = await LoadArtworkAsync(mediaProps.Thumbnail);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MediaController] Update error: {ex.Message}");
        }

        CurrentInfo = newInfo;

        if (isPlaybackOnly)
            PlaybackChanged?.Invoke(this, new MediaChangedEventArgs(newInfo));
        else
            MediaChanged?.Invoke(this, new MediaChangedEventArgs(newInfo));
    }

    private static async Task<BitmapSource?> LoadArtworkAsync(IRandomAccessStreamReference thumbnailRef)
    {
        try
        {
            using var stream = await thumbnailRef.OpenReadAsync();
            if (stream == null || stream.Size == 0) return null;

            using var netStream = stream.AsStreamForRead();
            using var ms = new MemoryStream();
            await netStream.CopyToAsync(ms);
            ms.Position = 0;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.StreamSource = ms;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MediaController] Artwork decode error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> PlayPauseAsync()
    {
        if (_currentSession == null) return false;
        try
        {
            var playbackInfo = _currentSession.GetPlaybackInfo();
            if (playbackInfo != null && playbackInfo.PlaybackStatus == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing)
            {
                return await _currentSession.TryPauseAsync();
            }
            else
            {
                return await _currentSession.TryPlayAsync();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MediaController] PlayPause error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> NextAsync()
    {
        if (_currentSession == null) return false;
        try
        {
            return await _currentSession.TrySkipNextAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MediaController] Next error: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> PreviousAsync()
    {
        if (_currentSession == null) return false;
        try
        {
            return await _currentSession.TrySkipPreviousAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[MediaController] Previous error: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_sessionManager != null)
        {
            _sessionManager.CurrentSessionChanged -= OnCurrentSessionChanged;
            _sessionManager.SessionsChanged -= OnSessionsChanged;
        }

        lock (_lock)
        {
            if (_currentSession != null)
            {
                _currentSession.MediaPropertiesChanged -= OnMediaPropertiesChanged;
                _currentSession.PlaybackInfoChanged -= OnPlaybackInfoChanged;
                _currentSession = null;
            }
        }
    }
}
