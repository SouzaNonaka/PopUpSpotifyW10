using System.Runtime.InteropServices;
using SpotifyMediaFlyout.Core.Audio.Native;

namespace SpotifyMediaFlyout.Core.Audio;

public class VolumeChangedEventArgs : EventArgs
{
    public int Volume { get; }
    public bool IsMuted { get; }

    public VolumeChangedEventArgs(int volume, bool isMuted)
    {
        Volume = volume;
        IsMuted = isMuted;
    }
}

public sealed class AudioController : IDisposable
{
    private static Guid IID_IAudioEndpointVolume = typeof(IAudioEndpointVolume).GUID;
    private static Guid EmptyContext = Guid.Empty;

    private IAudioEndpointVolume? _endpointVolume;
    private VolumeCallback? _callback;
    private bool _disposed;

    public event EventHandler<VolumeChangedEventArgs>? VolumeChanged;

    public AudioController()
    {
        Initialize();
    }

    private void Initialize()
    {
        try
        {
            var enumerator = (IMMDeviceEnumerator)new MMDeviceEnumerator();
            int hr = enumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out var device);
            if (hr != 0 || device == null)
            {
                return;
            }

            hr = device.Activate(ref IID_IAudioEndpointVolume, 0, IntPtr.Zero, out var endpointObj);
            if (hr != 0 || endpointObj is not IAudioEndpointVolume endpoint)
            {
                return;
            }

            _endpointVolume = endpoint;
            _callback = new VolumeCallback(this);
            _endpointVolume.RegisterControlChangeNotify(_callback);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioController] Init failed: {ex.Message}");
        }
    }

    public int GetVolume()
    {
        if (_endpointVolume == null)
        {
            Initialize();
        }
        if (_endpointVolume == null) return 0;
        try
        {
            _endpointVolume.GetMasterVolumeLevelScalar(out float level);
            return (int)Math.Round(level * 100);
        }
        catch
        {
            return 0;
        }
    }

    public void SetVolume(int volumePercent)
    {
        if (_endpointVolume == null) Initialize();
        if (_endpointVolume == null) return;
        try
        {
            float level = Math.Clamp(volumePercent / 100.0f, 0.0f, 1.0f);
            _endpointVolume.SetMasterVolumeLevelScalar(level, ref EmptyContext);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioController] SetVolume error: {ex.Message}");
        }
    }

    public void IncreaseVolume(int step = 2)
    {
        int current = GetVolume();
        SetVolume(Math.Min(100, current + step));
    }

    public void DecreaseVolume(int step = 2)
    {
        int current = GetVolume();
        SetVolume(Math.Max(0, current - step));
    }

    public bool GetMuteState()
    {
        if (_endpointVolume == null) Initialize();
        if (_endpointVolume == null) return false;
        try
        {
            _endpointVolume.GetMute(out bool isMuted);
            return isMuted;
        }
        catch
        {
            return false;
        }
    }

    public void ToggleMute()
    {
        if (_endpointVolume == null) Initialize();
        if (_endpointVolume == null) return;
        try
        {
            bool currentMute = GetMuteState();
            _endpointVolume.SetMute(!currentMute, ref EmptyContext);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AudioController] ToggleMute error: {ex.Message}");
        }
    }

    internal void NotifyVolumeChanged(float volumeScalar, bool isMuted)
    {
        int volume = (int)Math.Round(volumeScalar * 100);
        VolumeChanged?.Invoke(this, new VolumeChangedEventArgs(volume, isMuted));
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        if (_endpointVolume != null && _callback != null)
        {
            try
            {
                _endpointVolume.UnregisterControlChangeNotify(_callback);
            }
            catch { }
        }

        _endpointVolume = null;
        _callback = null;
    }

    private class VolumeCallback : IAudioEndpointVolumeCallback
    {
        private readonly AudioController _controller;

        public VolumeCallback(AudioController controller)
        {
            _controller = controller;
        }

        public int OnNotify(IntPtr pNotify)
        {
            if (pNotify == IntPtr.Zero) return 0;
            try
            {
                var data = Marshal.PtrToStructure<AUDIO_VOLUME_NOTIFICATION_DATA>(pNotify);
                _controller.NotifyVolumeChanged(data.fMasterVolume, data.bMuted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VolumeCallback] Error: {ex.Message}");
            }
            return 0;
        }
    }
}
