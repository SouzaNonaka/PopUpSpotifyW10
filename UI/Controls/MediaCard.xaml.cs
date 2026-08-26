using System.Windows;
using System.Windows.Controls;
using SpotifyMediaFlyout.Core.State;

namespace SpotifyMediaFlyout.UI.Controls;

public partial class MediaCard : System.Windows.Controls.UserControl
{
    public event EventHandler? PreviousClicked;
    public event EventHandler? PlayPauseClicked;
    public event EventHandler? NextClicked;

    public MediaCard()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
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
            UpdateVisuals();
        });
    }

    public void UpdateVisuals()
    {
        if (DataContext is not FlyoutState state) return;

        if (!state.IsSpotifyAvailable && string.IsNullOrEmpty(state.Title))
        {
            Visibility = Visibility.Collapsed;
            ActiveLayout.Visibility = Visibility.Collapsed;
            InactiveLayout.Visibility = Visibility.Visible;
            return;
        }

        Visibility = Visibility.Visible;
        ActiveLayout.Visibility = Visibility.Visible;
        InactiveLayout.Visibility = Visibility.Collapsed;

        TxtTitle.Text = string.IsNullOrWhiteSpace(state.Title) ? "Sem informações" : state.Title;
        TxtArtist.Text = string.IsNullOrWhiteSpace(state.Artist) ? "Spotify" : state.Artist;

        if (state.IsPlaying)
        {
            PlayIcon.Visibility = Visibility.Collapsed;
            PauseIcon.Visibility = Visibility.Visible;
        }
        else
        {
            PlayIcon.Visibility = Visibility.Visible;
            PauseIcon.Visibility = Visibility.Collapsed;
        }

        ImgArtwork.Source = state.Artwork;

        BtnPrevious.IsEnabled = state.CanPrevious;
        BtnPlayPause.IsEnabled = state.CanPlayPause;
        BtnNext.IsEnabled = state.CanNext;
    }

    private void OnPreviousClick(object sender, RoutedEventArgs e)
    {
        PreviousClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnPlayPauseClick(object sender, RoutedEventArgs e)
    {
        PlayPauseClicked?.Invoke(this, EventArgs.Empty);
    }

    private void OnNextClick(object sender, RoutedEventArgs e)
    {
        NextClicked?.Invoke(this, EventArgs.Empty);
    }
}
