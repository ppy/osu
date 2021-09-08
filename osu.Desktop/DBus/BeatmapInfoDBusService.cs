using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using Tmds.DBus;

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
        private string _FullName = "Not Set?";

        public string FullName
        {
            get => _FullName;
            set
            {
                _FullName = value;
                dictionary[nameof(FullName)] = value;
            }
        }

        private string _Version = string.Empty;

        public string Version
        {
            get => _Version;
            set
            {
                _Version = value;
                dictionary[nameof(Version)] = value;
            }
        }

        private double _Stars;

        public double Stars
        {
            get => _Stars;
            set
            {
                _Stars = value;
                dictionary[nameof(Stars)] = value;
            }
        }

        private int _OnlineID;

        public int OnlineID
        {
            get => _OnlineID;
            set
            {
                _OnlineID = value;
                dictionary[nameof(OnlineID)] = value;
            }
        }

        private double _BPM;

        public double BPM
        {
            get => _BPM;
            set
            {
                _BPM = value;
                dictionary[nameof(BPM)] = value;
            }
        }

        private readonly IDictionary<string, object> dictionary = new ConcurrentDictionary<string, object>
        {
            [nameof(FullName)] = string.Empty,
            [nameof(Version)] = string.Empty,
            [nameof(Stars)] = default,
            [nameof(OnlineID)] = default,
            [nameof(BPM)] = default
        };

        internal IDictionary<string, object> ToDictionary() => dictionary;

        internal bool SetValue(string name, object value)
        {
            switch (name)
            {
                case nameof(FullName):
                    FullName = (string)value;
                    return true;

                case nameof(Version):
                    Version = (string)value;
                    return true;

                case nameof(Stars):
                    Stars = (double)value;
                    return true;

                case nameof(OnlineID):
                    OnlineID = (int)value;
                    return true;

                case nameof(BPM):
                    BPM = (double)value;
                    return true;
            }

            return false;
        }
    }

    public class BeatmapInfoDBusService : IBeatmapInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/CurrentBeatmap");

        private readonly BeatmapMetadataProperties properties = new BeatmapMetadataProperties();

        public WorkingBeatmap Beatmap
        {
            set
            {
                setProperty(nameof(properties.FullName), (value.Metadata.ArtistUnicode ?? value.Metadata.Artist)
                                                         + " - "
                                                         + (value.Metadata.TitleUnicode ?? value.Metadata.Title));

                setProperty(nameof(properties.Version), value.BeatmapInfo.Version ?? "???");
                setProperty(nameof(properties.Stars), value.BeatmapInfo.StarDifficulty);
                setProperty(nameof(properties.OnlineID), value.BeatmapInfo.OnlineBeatmapID ?? -1);
                setProperty(nameof(properties.BPM), value.BeatmapInfo.BPM);
            }
        }

        private void setProperty(string target, object value)
        {
            if (properties.SetValue(target, value))
                OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(nameof(target), value));
        }

        public Task<BeatmapMetadataProperties> GetAllAsync()
            => Task.FromResult(properties);

        public Task<object> GetAsync(string prop)
            => Task.FromResult(properties.ToDictionary()[prop]);

        public Task SetAsync(string prop, object val)
        {
            throw new InvalidOperationException("只读属性");
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
            => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);

        public Task<string> GetFullNameAsync()
            => Task.FromResult(properties.FullName);

        public Task<string> GetCurrentDifficultyVersionAsync()
            => Task.FromResult(properties.Version);

        public Task<double> GetCurrentDifficultyStarAsync()
            => Task.FromResult(properties.Stars);

        public Task<int> GetOnlineIdAsync()
            => Task.FromResult(properties.OnlineID);

        public Task<double> GetBPMAsync()
            => Task.FromResult(properties.BPM);
    }
}
