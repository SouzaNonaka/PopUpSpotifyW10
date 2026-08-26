using System.Windows.Media.Imaging;
using Windows.Media.Control;

namespace SpotifyMediaFlyout.Core.Media;

public class MediaSessionInfo
{
    public bool IsAvailable { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public string Album { get; set; } = string.Empty;
    public BitmapSource? Artwork { get; set; }
    public GlobalSystemMediaTransportControlsSessionPlaybackStatus PlaybackStatus { get; set; }
        = GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed;
    public bool CanPlayPause { get; set; }
    public bool CanNext { get; set; }
    public bool CanPrevious { get; set; }
}

public class MediaChangedEventArgs : EventArgs
{
    public MediaSessionInfo MediaInfo { get; }

    public MediaChangedEventArgs(MediaSessionInfo mediaInfo)
    {
        MediaInfo = mediaInfo;
    }
}
