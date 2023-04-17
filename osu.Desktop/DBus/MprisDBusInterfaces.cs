using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using M.DBus;
using Tmds.DBus;

#nullable disable

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

        private readonly string[] ignoreList =
        {
            nameof(PlaybackStatus),
            nameof(LoopStatus)
        };

        public object Get(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.GetValueFor(this, prop, members);
        }

        internal bool Set(string name, object newValue, bool isExternal)
        {
            if (isExternal && ignoreList.Contains(name)) return false;

            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.SetValueFor(this, name, newValue, members);
        }

        internal bool Contains(string prop)
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return ServiceUtils.CheckifContained(this, prop, members);
        }

        internal IDictionary<string, object> GetMembers()
        {
            ServiceUtils.CheckIfDirectoryNotReady(this, members, out members);
            return members;
        }

        internal string _PlaybackStatus = "Paused";

        public string PlaybackStatus
        {
            get => _PlaybackStatus;
            set => _PlaybackStatus = value;
        }

        internal string _LoopStatus = "Single";

        public string LoopStatus
        {
            get => _LoopStatus;
            set => _LoopStatus = value;
        }

        private double _Rate = 1.0;

        public double Rate
        {
            get => _Rate;
            set => _Rate = value;
        }

        private bool _Shuffle = false;

        public bool Shuffle
        {
            get => _Shuffle;
            set => _Shuffle = value;
        }

        private readonly IDictionary<string, object> _Metadata = new Dictionary<string, object>
        {
            ["xesam:artist"] = new[]
            {
                "艺术家"
            },
            ["xesam:title"] = "标题",
            ["xesam:trackNumber"] = 1,
            ["mpris:length"] = (long)1
        };

        public IDictionary<string, object> Metadata => _Metadata;

        private double _Volume = 1.0;

        public double Volume
        {
            get => _Volume;
            set => _Volume = value;
        }

        private long _Position;

        public long Position
        {
            get => _Position;
            set => _Position = value;
        }

        internal long TrackLength
        {
            set => _Metadata["mpris:length"] = value;
        }

        private readonly double _MinimumRate = 1;

        public double MinimumRate => _MinimumRate;

        private readonly double _MaximumRate = 1;

        public double MaximumRate => _MaximumRate;

        private bool _CanGoNext = true;

        public bool CanGoNext
        {
            get => _CanGoNext;
            set => _CanGoNext = value;
        }

        private bool _CanGoPrevious = true;

        public bool CanGoPrevious
        {
            get => _CanGoPrevious;
            set => _CanGoPrevious = value;
        }

        private readonly bool _CanPlay = true;

        public bool CanPlay => _CanPlay;

        private readonly bool _CanPause = true;

        public bool CanPause => _CanPause;

        private bool _CanSeek = true;

        public bool CanSeek
        {
            get => _CanSeek;
            set => _CanSeek = value;
        }

        private readonly bool _CanControl = true;

        public bool CanControl => _CanControl;

        //https://github.com/linuxdeepin/dtkwidget/blob/82bbc6fb20b43c17a957b10ebfd586a90a4a909f/src/widgets/private/mpris/dbusmpris.h#L70
        private readonly bool _CanShowInUI = true;
        public bool CanShowInUI => _CanShowInUI;
    }
}
