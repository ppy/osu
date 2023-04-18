using System.Threading.Tasks;
using M.DBus;
using osu.Game.Beatmaps;
using Tmds.DBus;

#nullable enable

namespace osu.Desktop.DBus
{
    [DBusInterface("io.matrix_feather.mfosu.Audio")]
    public interface IAudioInfoDBusService : IDBusObject
    {
        Task<double> GetTrackLengthAsync();
        Task<double> GetTrackProgressAsync();
    }

    public class AudioInfoDBusService : IMDBusObject, IAudioInfoDBusService
    {
        public ObjectPath ObjectPath => PATH;
        public static readonly ObjectPath PATH = new ObjectPath("/io/matrix_feather/mfosu/Audio");

        public string? CustomRegisterName => null;
        public bool IsService => true;

        public WorkingBeatmap? Beatmap { get; set; }

        public Task<double> GetTrackLengthAsync()
            => Task.FromResult(Beatmap?.Track.Length ?? 0d);

        public Task<double> GetTrackProgressAsync()
            => Task.FromResult(Beatmap?.Track.CurrentTime ?? 0d);
    }
}
