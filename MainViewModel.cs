using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
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
            MuteUnmuteCommand = new RelayCommand<EventArgs>(MuteUnmute);
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
        public ICommand MuteUnmuteCommand { get; }

        private void MuteUnmute(EventArgs e)
        {
            MediaPlayer.ToggleMute();
            //MediaPlayer.Volume = 100; // By the time user is able to activate button, both a write or mute will do the trick
        }

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
            // Couldn't get this to work
            //string[] args = new string[3];
            //args[0] = eventArgs.SwapChainOptions[0];
            //args[1] = eventArgs.SwapChainOptions[1];
            //args[2] = "--no-volume-save";
            //LibVLC = new LibVLC(enableDebugLogs: false, args);

            LibVLC = new LibVLC(enableDebugLogs: false, eventArgs.SwapChainOptions);

            MediaPlayer = new MediaPlayer(LibVLC);
            MediaPlayer.Playing += MediaPlayer_Playing;
            MediaPlayer.VolumeChanged += (sender, e) => Debug.WriteLine("Volume changed! New volume: " + e.Volume);
            MediaPlayer.Muted += (sender, args) => Debug.WriteLine("Muted!");
            MediaPlayer.Unmuted += (sender, args) => Debug.WriteLine("Unmuted!");

            Debug.WriteLine("Volume after creating mediaplayer: " + MediaPlayer.Volume); // Is always 0
            //Debug.WriteLine("Setting volume to 20 percent");
            //MediaPlayer.Volume = 20; // Never works

            Thread queryThread = new Thread(new ThreadStart(Query));
            queryThread.Start();

            // Uncomment setThread.Start() to print timespan between creating mediaplayer and first time the property can and has been written to, usually 600-700 ms
            // Otherwise, print timespan between creating mediaplayer and the volume "registering" as a result from pressing the button
            Thread setThread = new Thread(new ThreadStart(SetVolume));
            //setThread.Start();

            using var media = new Media(LibVLC, new Uri("http://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4"));
            MediaPlayer.Play(media);
        }

        bool flag = false;

        private void Query()
        {
            DateTime start = DateTime.Now;
            DateTime not0 = DateTime.Now;
            while (MediaPlayer.Volume == 0)
            {
                not0 = DateTime.Now;
            }
            flag = true;
            Debug.WriteLine("Volume took " + not0.Subtract(start).TotalSeconds/*ToString(@"ss\.fff")*/ + " seconds since creating mediaplayer to register as non-zero");
        }

        private void SetVolume()
        {
            while (!flag)
            {
                MediaPlayer.Volume = 100;
            }
        }

        private void MediaPlayer_Playing(object sender, EventArgs e)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                Debug.WriteLine("Playing event! Volume: " + MediaPlayer.Volume); // Is always 0
            });
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
