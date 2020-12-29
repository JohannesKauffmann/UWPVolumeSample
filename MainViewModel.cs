using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using LibVLCSharp.Platforms.UWP;
using LibVLCSharp.Shared;

namespace LibVLCSharp.UWP.Sample
{
    /// <summary>
    /// Main view model
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initialized a new instance of <see cref="MainViewModel"/> class
        /// </summary>
        public MainViewModel()
        {
            InitializedCommand = new RelayCommand<InitializedEventArgs>(Initialize);
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~MainViewModel()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the command for the initialization
        /// </summary>
        public ICommand InitializedCommand { get; }

        private LibVLC LibVLC { get; set; }

        private MediaPlayer _mediaPlayer;
        /// <summary>
        /// Gets the media player
        /// </summary>
        public MediaPlayer MediaPlayer
        {
            get => _mediaPlayer;
            private set => Set(nameof(MediaPlayer), ref _mediaPlayer, value);
        }

        private void Set<T>(string propertyName, ref T field, T value)
        {
            if (field == null && value != null || field != null && !field.Equals(value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void Initialize(InitializedEventArgs eventArgs)
        {
            LibVLC = new LibVLC(enableDebugLogs: false, eventArgs.SwapChainOptions);
            MediaPlayer = new MediaPlayer(LibVLC);
            Debug.WriteLine("Volume after creating mediaplayer: " + MediaPlayer.Volume);
            Debug.WriteLine("Setting volume to 20 percent");
            MediaPlayer.Volume = 20;
            MediaPlayer.Playing += MediaPlayer_Playing;
            MediaPlayer.VolumeChanged += MediaPlayer_VolumeChanged;
            using var media = new Media(LibVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));
            MediaPlayer.Play(media);
        }

        private void MediaPlayer_VolumeChanged(object sender, MediaPlayerVolumeChangedEventArgs e)
        {
            Debug.WriteLine("Volume changed! New volume: " + e.Volume);
        }

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            Debug.WriteLine("Playing event! Volume: " + MediaPlayer.Volume);
        }

        /// <summary>
        /// Cleaning
        /// </summary>
        public void Dispose()
        {
            var mediaPlayer = MediaPlayer;
            MediaPlayer = null;
            mediaPlayer?.Dispose();
            LibVLC?.Dispose();
            LibVLC = null;
        }
    }
}
