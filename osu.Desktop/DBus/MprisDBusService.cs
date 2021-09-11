using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Bindables;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using Tmds.DBus;

namespace osu.Desktop.DBus
{
    public class MprisPlayerService : IPlayer, IMediaPlayer2
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/org/mpris/MediaPlayer2");

        private readonly PlayerProperties playerProperties = new PlayerProperties();
        private readonly MediaPlayer2Properties mp2Properties = new MediaPlayer2Properties();

        internal Storage Storage { get; set; }

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

        private bool trackRunning;

        /// <summary>
        /// 暂停、播放显示
        /// </summary>
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

        /// <summary>
        /// 元数据
        /// </summary>
        private WorkingBeatmap beatmap;

        internal WorkingBeatmap Beatmap
        {
            get => beatmap;
            set
            {
                beatmap = value;

                var info = value.BeatmapInfo;
                playerProperties.Metadata["xesam:artist"] = new[] { info.Metadata?.ArtistUnicode ?? info.Metadata?.Artist ?? string.Empty };
                playerProperties.Metadata["xesam:title"] = info.Metadata?.TitleUnicode ?? info.Metadata?.Title ?? string.Empty;
                playerProperties.Metadata["mpris:artUrl"] = resolveBeatmapCoverUrl(value);

                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("Metadata", playerProperties.Metadata));
            }
        }

        [CanBeNull]
        private Bindable<bool> useAvatarLogoAsDefault;

        internal Bindable<bool> UseAvatarLogoAsDefault
        {
            get => useAvatarLogoAsDefault;
            set
            {
                useAvatarLogoAsDefault?.UnbindAll();
                useAvatarLogoAsDefault = value;

                value.BindValueChanged(_ =>
                {
                    playerProperties.Metadata["mpris:artUrl"] = resolveBeatmapCoverUrl(beatmap);
                    OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty("Metadata", playerProperties.Metadata));
                }, true);
            }
        }

        private string resolveBeatmapCoverUrl(WorkingBeatmap beatmap)
        {
            const string head = "file://";
            string body;
            var backgroundFilename = beatmap?.BeatmapInfo.Metadata?.BackgroundFile;

            if (!string.IsNullOrEmpty(backgroundFilename) && !(UseAvatarLogoAsDefault?.Value ?? false))
            {
                body = Storage?.GetFullPath("files")
                       + Path.DirectorySeparatorChar
                       + (beatmap.BeatmapSetInfo.GetPathForFile(beatmap.BeatmapInfo.Metadata?.BackgroundFile)
                          ?? string.Empty);
            }
            else
            {
                var target = Storage?.GetFiles("custom", "avatarlogo*").FirstOrDefault(s => s.Contains("avatarlogo"));
                if (!string.IsNullOrEmpty(target))
                    body = Storage.GetFullPath(target);
                else
                    return string.Empty;
            }

            return head + body;
        }

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
            => Task.FromResult(mp2Properties.Contains(prop)
                ? mp2Properties.Get(prop)
                : playerProperties.Get(prop));

        public Task SetAsync(string prop, object val)
        {
            if (prop == nameof(PlayerProperties.Volume))
            {
                playerProperties.Set(nameof(PlayerProperties.Volume), val);
            }

            return Task.CompletedTask;
        }

        Task<MediaPlayer2Properties> IMediaPlayer2.GetAllAsync()
            => Task.FromResult(mp2Properties);

        public Task<PlayerProperties> GetAllAsync()
            => Task.FromResult(playerProperties);

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
            => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
    }
}
