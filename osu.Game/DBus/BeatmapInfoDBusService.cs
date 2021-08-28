using System.Threading.Tasks;
using osu.Game.Beatmaps;
using Tmds.DBus;

namespace osu.Game.DBus
{
    [DBusInterface("io.matrix_feather.mfosu.CurrentBeatmap")]
    public interface IBeatmapInfoDBusService : IDBusObject
    {
        Task<string> GetFullNameAsync();
        Task<double> GetTrackLengthAsync();
        Task<double> GetTrackProgressAsync();
        Task<string> GetCurrentDifficultyVersionAsync();
        Task<double> GetCurrentDifficultyStarAsync();
    }

    public class BeatmapInfoDBusService : IBeatmapInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/CurrentBeatmap");

        public WorkingBeatmap Beatmap { get; set; }

        public Task<string> GetFullNameAsync()
        {
            var info = (Beatmap.Metadata.ArtistUnicode ?? Beatmap.Metadata.Artist)
                       + " - "
                       + (Beatmap.Metadata.TitleUnicode ?? Beatmap.Metadata.Title);

            return Task.FromResult(info);
        }

        public Task<double> GetTrackLengthAsync()
            => Task.FromResult(Beatmap.Track.Length);

        public Task<double> GetTrackProgressAsync()
            => Task.FromResult(Beatmap.Track.CurrentTime);

        public Task<string> GetCurrentDifficultyVersionAsync()
            => Task.FromResult(Beatmap.BeatmapInfo.Version ?? "???");

        public Task<double> GetCurrentDifficultyStarAsync()
            => Task.FromResult(Beatmap.BeatmapInfo.StarDifficulty);
    }
}
