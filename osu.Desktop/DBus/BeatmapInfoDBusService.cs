using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using Tmds.DBus;

namespace osu.Desktop.DBus
{
    [DBusInterface("io.matrix_feather.mfosu.CurrentBeatmap")]
    public interface IBeatmapInfoDBusService : IDBusObject
    {
        Task<IDictionary<string, object>> GetAllAsync();
        Task<object> GetAsync(string prop);
        Task SetAsync(string prop, object val);
        Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler);
        Task<string> GetFullNameAsync();
        Task<string> GetCurrentDifficultyVersionAsync();
        Task<double> GetCurrentDifficultyStarAsync();
        Task<int> GetOnlineIdAsync();
        Task<double> GetBPMAsync();
    }

    public class BeatmapInfoDBusService : IBeatmapInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/CurrentBeatmap");

        public WorkingBeatmap Beatmap
        {
            set
            {
                setProperty("fullname",
                    (value.Metadata.ArtistUnicode ?? value.Metadata.Artist)
                    + " - "
                    + (value.Metadata.TitleUnicode ?? value.Metadata.Title));

                setProperty("difficulty", value.BeatmapInfo.Version ?? "???");
                setProperty("stars", value.BeatmapInfo.StarDifficulty);
                setProperty("online_id", value.BeatmapInfo.OnlineBeatmapID ?? -1);
                setProperty("bpm", value.BeatmapInfo.BPM);
            }
        }

        private void setProperty(string name, object value)
        {
            properties[name] = value;
            OnPropertiesChanged?.Invoke(PropertyChanges.ForProperty(name, value));
        }

        private readonly IDictionary<string, object> properties = new ConcurrentDictionary<string, object>
        {
            ["fullname"] = string.Empty,
            ["difficulty"] = string.Empty,
            ["stars"] = 0.0d,
            ["online_id"] = 0,
            ["bpm"] = 0
        };

        public Task<IDictionary<string, object>> GetAllAsync()
            => Task.FromResult(properties);

        public Task<object> GetAsync(string prop)
            => Task.FromResult(properties[prop]);

        public Task SetAsync(string prop, object val)
        {
            throw new NotImplementedException();
        }

        public event Action<PropertyChanges> OnPropertiesChanged;

        public Task<IDisposable> WatchPropertiesAsync(Action<PropertyChanges> handler)
            => SignalWatcher.AddAsync(this, nameof(OnPropertiesChanged), handler);

        public Task<string> GetFullNameAsync()
            => Task.FromResult(properties["fullname"] as string);

        public Task<string> GetCurrentDifficultyVersionAsync()
            => Task.FromResult(properties["difficulty"] as string);

        public Task<double> GetCurrentDifficultyStarAsync()
            => Task.FromResult(properties["stars"] as double? ?? 0);

        public Task<int> GetOnlineIdAsync()
            => Task.FromResult(properties["online_id"] as int? ?? 0);

        public Task<double> GetBPMAsync()
            => Task.FromResult(properties["bpm"] as double? ?? 0);
    }
}
