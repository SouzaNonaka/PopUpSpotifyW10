using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SpotifyMediaFlyout.Core.State;

namespace SpotifyMediaFlyout.UI.Controls;

public partial class VolumeCard : System.Windows.Controls.UserControl
{
    private bool _isInternalSliderChange = false;

    public event EventHandler? MuteToggled;
    public event EventHandler<int>? VolumeChangedByCard;

    public VolumeCard()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        MouseWheel += OnCardMouseWheel;
        Loaded += (s, e) => UpdateVisuals();
    }

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue is FlyoutState oldState)
        {
            oldState.PropertyChanged -= OnStatePropertyChanged;
        }

        if (e.NewValue is FlyoutState newState)
        {
            newState.PropertyChanged += OnStatePropertyChanged;
            UpdateVisuals();
        }
    }

    private void OnStatePropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Dispatcher.InvokeAsync(() =>
        {
            if (e.PropertyName is nameof(FlyoutState.Volume) or nameof(FlyoutState.IsMuted) or null)
            {
                UpdateVisuals();
            }
        });
    }

    public void UpdateVisuals()
    {
        if (DataContext is not FlyoutState state) return;

        TxtVolume.Text = state.Volume.ToString();

        _isInternalSliderChange = true;
        VolumeSlider.Value = state.Volume;
        _isInternalSliderChange = false;

        if (state.IsMuted || state.Volume == 0)
        {
            WaveLow.Visibility = Visibility.Collapsed;
            WaveHigh.Visibility = Visibility.Collapsed;
            MuteSlash.Visibility = Visibility.Visible;
        }
        else if (state.Volume < 33)
        {
            WaveLow.Visibility = Visibility.Visible;
            WaveHigh.Visibility = Visibility.Collapsed;
            MuteSlash.Visibility = Visibility.Collapsed;
        }
        else
        {
            WaveLow.Visibility = Visibility.Visible;
            WaveHigh.Visibility = Visibility.Visible;
            MuteSlash.Visibility = Visibility.Collapsed;
        }
    }

    private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_isInternalSliderChange) return;

        int newVol = (int)Math.Round(e.NewValue);
        VolumeChangedByCard?.Invoke(this, newVol);
    }

    private void OnMuteClick(object sender, RoutedEventArgs e)
    {
        MuteToggled?.Invoke(this, EventArgs.Empty);
    }

    private void OnCardMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (DataContext is not FlyoutState state) return;

        int step = e.Delta > 0 ? 2 : -2;
        int newVol = Math.Clamp(state.Volume + step, 0, 100);
        VolumeChangedByCard?.Invoke(this, newVol);
        e.Handled = true;
    }
}
