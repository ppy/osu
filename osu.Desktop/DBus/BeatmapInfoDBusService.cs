using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using M.DBus;
using osu.Framework.Platform;
using osu.Game.Beatmaps;
using osu.Game.Screens.LLin.Misc;
using Tmds.DBus;

#nullable disable

namespace osu.Desktop.DBus
{
    [DBusInterface("io.matrix_feather.mfosu.CurrentBeatmap")]
    public interface IBeatmapInfoDBusService : IDBusObject
    {
        Task<BeatmapMetadataProperties> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
        Task<string> GetFullNameAsync();
        Task<string> GetCurrentDifficultyVersionAsync();
        Task<double> GetCurrentDifficultyStarAsync();
        Task<int> GetOnlineIdAsync();
        Task<double> GetBPMAsync();
    }

    [Dictionary]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "ConvertToAutoProperty")]
    [SuppressMessage("ReSharper", "ConvertToConstant.Local")]
    [SuppressMessage("ReSharper", "ConvertToAutoPropertyWhenPossible")]
    public class BeatmapMetadataProperties
    {
        public string FullName
        {
            get => _FullName;
            set => _FullName = value;
        }

        public string Version
        {
            get => _Version;
            set => _Version = value;
        }

        public double Stars
        {
            get => _Stars;
            set => _Stars = value;
        }

        public int OnlineID
        {
            get => _OnlineID;
            set => _OnlineID = value;
        }

        public double BPM
        {
            get => _BPM;
            set => _BPM = value;
        }

        public string CoverPath
        {
            get => _CoverPath;
            set => _CoverPath = value;
        }

        private string _FullName = "Not Set?";

        private string _Version = string.Empty;

        private double _Stars;

        private int _OnlineID;

        private double _BPM;

        private string _CoverPath;

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

    public class BeatmapInfoDBusService : IMDBusObject, IBeatmapInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/CurrentBeatmap");

        public string CustomRegisterName { get; } = string.Empty;

        public WorkingBeatmap Beatmap
        {
            set
            {
                setProperty(nameof(properties.FullName), value.Metadata.GetTitle() + " - " + value.Metadata.GetArtist());

                setProperty(nameof(properties.Version), value.BeatmapInfo.DifficultyName);
                setProperty(nameof(properties.Stars), value.BeatmapInfo.StarRating);
                setProperty(nameof(properties.OnlineID), value.BeatmapInfo.OnlineID);
                setProperty(nameof(properties.BPM), value.BeatmapInfo.BPM);
                setProperty(nameof(properties.CoverPath), resolveBeatmapCoverUrl(value));
            }
        }

        internal Storage Storage { get; set; }

        private readonly BeatmapMetadataProperties properties = new BeatmapMetadataProperties();

        public Task<BeatmapMetadataProperties> GetAllAsync()
        {
            return Task.FromResult(properties);
        }

        public Task<object> GetAsync(string prop)
        {
            return Task.FromResult(properties.Get(prop));
        }

        public Task SetAsync(string prop, object val)
        {
            return Task.FromException(new InvalidOperationException("只读属性"));
        }

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
        {
            return SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);
        }

        public Task<string> GetFullNameAsync()
        {
            return Task.FromResult(properties.FullName);
        }

        public Task<string> GetCurrentDifficultyVersionAsync()
        {
            return Task.FromResult(properties.Version);
        }

        public Task<double> GetCurrentDifficultyStarAsync()
        {
            return Task.FromResult(properties.Stars);
        }

        public Task<int> GetOnlineIdAsync()
        {
            return Task.FromResult(properties.OnlineID);
        }

        public Task<double> GetBPMAsync()
        {
            return Task.FromResult(properties.BPM);
        }

        private string resolveBeatmapCoverUrl(WorkingBeatmap beatmap)
        {
            string body;
            string backgroundFilename = beatmap?.BeatmapInfo.Metadata?.BackgroundFile;

            if (!string.IsNullOrEmpty(backgroundFilename))
            {
                body = Storage?.GetFullPath("files")
                       + Path.DirectorySeparatorChar
                       + (beatmap.BeatmapSetInfo.GetPathForFile(beatmap.BeatmapInfo.Metadata?.BackgroundFile)
                          ?? string.Empty);
            }
            else
            {
                string targetPath = Storage?.GetFiles("custom", "avatarlogo*").FirstOrDefault(s => s.Contains("avatarlogo"));

                if (!string.IsNullOrEmpty(targetPath))
                    body = Storage.GetFullPath(targetPath);
                else
                    return string.Empty;
            }

            return body;
        }

        private void setProperty(string target, object value)
        {
            if (properties.Set(target, value))
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(target, value));
        }

        public event Action<PropertyChanges> OnPropertiesChanged;
    }
}
