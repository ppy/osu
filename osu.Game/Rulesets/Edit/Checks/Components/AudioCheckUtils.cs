using System.Linq;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public static class AudioCheckUtils
    {
        public static readonly string[] AUDIO_EXTENSIONS = { "mp3", "ogg", "wav" };

        public static bool HasAudioExtension(string filename) => AUDIO_EXTENSIONS.Any(filename.ToLowerInvariant().EndsWith);
    }
}