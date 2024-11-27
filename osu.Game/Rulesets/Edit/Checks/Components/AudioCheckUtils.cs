// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public static class AudioCheckUtils
    {
        public static readonly string[] AUDIO_EXTENSIONS = { "mp3", "ogg", "wav" };

        public static bool HasAudioExtension(string filename) => AUDIO_EXTENSIONS.Any(Path.GetExtension(filename).ToLowerInvariant().EndsWith);
    }
}
