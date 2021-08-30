using System;
using System.IO;
using System.Threading.Tasks;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using Tmds.DBus;

namespace osu.Game.DBus
{
    public class MprisPlayerService : IPlayer, IMediaPlayer2
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/org/mpris/MediaPlayer2");

        private readonly PlayerProperties playerProperties = new PlayerProperties();
        private readonly MediaPlayer2Properties mp2Properties = new MediaPlayer2Properties();

        internal Storage Storage { get; set; }

        private bool trackRunning;

        internal bool TrackRunning
        {
            set
            {
                if (trackRunning == value) return;

                trackRunning = value;

                playerProperties._PlaybackStatus = value ? "Playing" : "Paused";
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("PlaybackStatus", playerProperties.PlaybackStatus));
            }
        }

        internal WorkingBeatmap Beatmap
        {
            set
            {
                var info = value.BeatmapInfo;
                playerProperties.Metadata["xesam:artist"] = new[] { info.Metadata?.ArtistUnicode ?? info.Metadata?.Artist ?? string.Empty };
                playerProperties.Metadata["xesam:title"] = info.Metadata?.TitleUnicode ?? info.Metadata?.Title ?? string.Empty;
                playerProperties.Metadata["mpris:artUrl"] = "file://"
                                                            + Storage?.GetFullPath("files")
                                                            + Path.DirectorySeparatorChar
                                                            + (value.BeatmapSetInfo.GetPathForFile(info.Metadata?.BackgroundFile)
                                                               ?? string.Empty);

                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("Metadata", playerProperties.Metadata));
            }
        }

        #region 用于导出到游戏的控制

        public Action Next;
        public Action Previous;
        public Action Pause;
        public Action PlayPause;
        public Action Stop;
        public Action Play;
        public Action<double> Seek;
        public Action<string> OpenUri;

        public Action WindowRaise;
        public Action Quit;

        #endregion

        private void triggerPauseChange()
            => OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("PlaybackStatus", playerProperties.PlaybackStatus));

        public Task NextAsync()
        {
            Next?.Invoke();
            return Task.CompletedTask;
        }

        public Task PreviousAsync()
        {
            Previous?.Invoke();
            return Task.CompletedTask;
        }

        public Task PauseAsync()
        {
            Pause?.Invoke();
            return Task.CompletedTask;
        }

        public Task PlayPauseAsync()
        {
            PlayPause?.Invoke();
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            Stop?.Invoke();
            return Task.CompletedTask;
        }

        public Task PlayAsync()
        {
            Play?.Invoke();
            playerProperties._PlaybackStatus = "Playing";
            triggerPauseChange();
            return Task.CompletedTask;
        }

        public Task SeekAsync(long offset)
        {
            Seek?.Invoke(offset);
            return Task.CompletedTask;
        }

        public Task SetPositionAsync(ObjectPath trackId, long position)
        {
            return Task.CompletedTask;
        }

        public Task OpenUriAsync(string uri)
        {
            OpenUri?.Invoke(uri);
            return Task.CompletedTask;
        }

        public event Action<long> OnSeeked;

        public Task<IDisposable> WatchSeekedAsync(Action<long> handler, Action<Exception> onError = null)
            => SignalWatcher.AddAsync(this, nameof(OnSeeked), handler);

        public Task RaiseAsync()
        {
            WindowRaise?.Invoke();
            return Task.CompletedTask;
        }

        public Task QuitAsync()
        {
            Quit?.Invoke();
            return Task.CompletedTask;
        }

        public Task<object> GetAsync(string prop)
        {
            var playerDict = playerProperties.ToDictionary();
            var mp2Dict = mp2Properties.ToDictionary();

            if (playerDict.ContainsKey(prop))
                return Task.FromResult(playerDict[prop]);
            else
                return Task.FromResult(mp2Dict[prop]);
        }

        Task<MediaPlayer2Properties> IMediaPlayer2.GetAllAsync()
            => Task.FromResult(mp2Properties);

        public Task<PlayerProperties> GetAllAsync()
            => Task.FromResult(playerProperties);

        public Task SetAsync(string prop, object val)
            => Task.CompletedTask;

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
            => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }
}
