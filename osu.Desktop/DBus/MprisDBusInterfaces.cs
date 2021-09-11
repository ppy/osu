using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using M.DBus;
using Tmds.DBus;

namespace osu.Desktop.DBus
{
    [DBusInterface("org.mpris.MediaPlayer2")]
    public interface IMediaPlayer2 : IDBusObject
    {
        Task RaiseAsync();
        Task QuitAsync();
        Task<object> GetAsync(string prop);
        Task<MediaPlayer2Properties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class MediaPlayer2Properties
    {
        private readonly bool _CanQuit = true;

        public bool CanQuit => _CanQuit;

        private readonly bool _Fullscreen = false;

        public bool Fullscreen => _Fullscreen;

        private readonly bool _CanSetFullscreen = false;

        public bool CanSetFullscreen => _CanSetFullscreen;

        private readonly bool _CanRaise = true;

        public bool CanRaise => _CanRaise;

        private readonly bool _HasTrackList = false;

        public bool HasTrackList => _HasTrackList;

        private readonly string _Identity = "mfosu";

        public string Identity => _Identity;

        private readonly string _DesktopEntry = "mfosu";

        public string DesktopEntry => _DesktopEntry;

        private readonly string[] _SupportedUriSchemes =
        {
            "osu://"
        };

        public string[] SupportedUriSchemes => _SupportedUriSchemes;

        private readonly string[] _SupportedMimeTypes = Array.Empty<string>();

        public string[] SupportedMimeTypes => _SupportedMimeTypes;

        private IDictionary<string, object> members;

        public object Get(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.GetValueFor(this, prop, members);
        }

        internal bool Set(string name, object newValue)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.SetValueFor(this, name, newValue, members);
        }

        internal bool Contains(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.CheckifContained(this, prop, members);
        }
    }

    [DBusInterface("org.mpris.MediaPlayer2.Player")]
    public interface IPlayer : IDBusObject
    {
        Task NextAsync();
        Task PreviousAsync();
        Task PauseAsync();
        Task PlayPauseAsync();
        Task StopAsync();
        Task PlayAsync();
        Task SeekAsync(long offset);
        Task SetPositionAsync(ObjectPath trackId, long position);
        Task OpenUriAsync(string uri);
        Task<IDisposable> WatchSeekedAsync(Action<long> handler, Action<Exception> onError = null);
        Task<object> GetAsync(string prop);
        Task<PlayerProperties> GetAllAsync();
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
    }

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class PlayerProperties
    {
        private IDictionary<string, object> members;

        public object Get(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.GetValueFor(this, prop, members);
        }

        internal bool Set(string name, object newValue)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.SetValueFor(this, name, newValue, members);
        }

        internal bool Contains(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.CheckifContained(this, prop, members);
        }

        internal string _PlaybackStatus = "Paused";

        public string PlaybackStatus => _PlaybackStatus;

        internal string _LoopStatus = "Single";

        public string LoopStatus => _LoopStatus;

        private readonly double _Rate = 1.0;

        public double Rate => _Rate;

        private readonly bool _Shuffle = false;

        public bool Shuffle => _Shuffle;

        private readonly IDictionary<string, object> _Metadata = new Dictionary<string, object>
        {
            ["xesam:artist"] = new[]
            {
                "艺术家"
            },
            ["xesam:title"] = "标题",
            ["xesam:trackNumber"] = 1,
        };

        public IDictionary<string, object> Metadata => _Metadata;

        private double _Volume = 0.0;

        public double Volume
        {
            get => _Volume;
            set => _Volume = value;
        }

        private readonly long _Position = 0;

        public long Position => _Position;

        private readonly double _MinimumRate = 1;

        public double MinimumRate => _MinimumRate;

        private readonly double _MaximumRate = 1;

        public double MaximumRate => _MaximumRate;

        private readonly bool _CanGoNext = true;

        public bool CanGoNext => _CanGoNext;

        private readonly bool _CanGoPrevious = true;

        public bool CanGoPrevious => _CanGoPrevious;

        private readonly bool _CanPlay = true;

        public bool CanPlay => _CanPlay;

        private readonly bool _CanPause = true;

        public bool CanPause => _CanPause;

        private readonly bool _CanSeek = false;

        public bool CanSeek => _CanSeek;

        private readonly bool _CanControl = true;

        public bool CanControl => _CanControl;
    }
}
