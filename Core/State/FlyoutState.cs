using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using Windows.Media.Control;

namespace SpotifyMediaFlyout.Core.State;

public class FlyoutState : INotifyPropertyChanged
{
    private int _volume = 70;
    private bool _isMuted = false;
    private bool _isSpotifyAvailable = false;
    private string _title = string.Empty;
    private string _artist = string.Empty;
    private string _album = string.Empty;
    private ImageSource? _artwork;
    private GlobalSystemMediaTransportControlsSessionPlaybackStatus _playbackState =
        GlobalSystemMediaTransportControlsSessionPlaybackStatus.Closed;
    private bool _canPlayPause = true;
    private bool _canNext = true;
    private bool _canPrevious = true;

    public int Volume
    {
        get => _volume;
        set
        {
            if (_volume != value)
            {
                _volume = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VolumeText));
                OnPropertyChanged(nameof(VolumeIconKind));
            }
        }
    }

    public string VolumeText => $"{Volume}";

    public bool IsMuted
    {
        get => _isMuted;
        set
        {
            if (_isMuted != value)
            {
                _isMuted = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(VolumeIconKind));
            }
        }
    }

    public string VolumeIconKind
    {
        get
        {
            if (IsMuted || Volume == 0) return "Mute";
            if (Volume < 33) return "Low";
            if (Volume < 66) return "Medium";
            return "High";
        }
    }

    public bool IsSpotifyAvailable
    {
        get => _isSpotifyAvailable;
        set
        {
            if (_isSpotifyAvailable != value)
            {
                _isSpotifyAvailable = value;
                OnPropertyChanged();
            }
        }
    }

    public string Title
    {
        get => _title;
        set
        {
            if (_title != value)
            {
                _title = value;
                OnPropertyChanged();
            }
        }
    }

    public string Artist
    {
        get => _artist;
        set
        {
            if (_artist != value)
            {
                _artist = value;
                OnPropertyChanged();
            }
        }
    }

    public string Album
    {
        get => _album;
        set
        {
            if (_album != value)
            {
                _album = value;
                OnPropertyChanged();
            }
        }
    }

    public ImageSource? Artwork
    {
        get => _artwork;
        set
        {
            if (_artwork != value)
            {
                _artwork = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasArtwork));
            }
        }
    }

    public bool HasArtwork => Artwork != null;

    public GlobalSystemMediaTransportControlsSessionPlaybackStatus PlaybackState
    {
        get => _playbackState;
        set
        {
            if (_playbackState != value)
            {
                _playbackState = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsPlaying));
                OnPropertyChanged(nameof(PlayPauseIcon));
            }
        }
    }

    public bool IsPlaying => PlaybackState == GlobalSystemMediaTransportControlsSessionPlaybackStatus.Playing;

    public string PlayPauseIcon => IsPlaying ? "Pause" : "Play";

    public bool CanPlayPause
    {
        get => _canPlayPause;
        set
        {
            if (_canPlayPause != value)
            {
                _canPlayPause = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanNext
    {
        get => _canNext;
        set
        {
            if (_canNext != value)
            {
                _canNext = value;
                OnPropertyChanged();
            }
        }
    }

    public bool CanPrevious
    {
        get => _canPrevious;
        set
        {
            if (_canPrevious != value)
            {
                _canPrevious = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
