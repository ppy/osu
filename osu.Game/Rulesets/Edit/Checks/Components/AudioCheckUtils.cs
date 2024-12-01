// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public static class AudioCheckUtils
    {
        public static bool HasAudioExtension(string filename) => SupportedExtensions.AUDIO_EXTENSIONS.Contains(Path.GetExtension(filename).ToLowerInvariant());
    }
}
