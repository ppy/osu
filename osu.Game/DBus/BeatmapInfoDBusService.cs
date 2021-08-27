using System.Threading.Tasks;
using osu.Game.Beatmaps;
using Tmds.DBus;

namespace osu.Game.DBus
{
    [DBusInterface("io.matrix_feather.mfosu.CurrentBeatmap")]
    public interface IBeatmapInfoDBusService : IDBusObject
    {
        Task<string> GetCurrentBeatmapInfoAsync();
    }

    public class BeatmapInfoDBusService : IBeatmapInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/CurrentBeatmap");

        public WorkingBeatmap Beatmap { get; set; }

        public Task<string> GetCurrentBeatmapInfoAsync()
        {
            var info = (Beatmap.Metadata.ArtistUnicode ?? Beatmap.Metadata.Artist)
                       + " - "
                       + (Beatmap.Metadata.TitleUnicode ?? Beatmap.Metadata.Title);

            return Task.FromResult(info);
        }
    }
}
