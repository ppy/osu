// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Taiko
{
    public static class TaikoEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Osu.OsuEditor";

        /// <summary>
        /// "Hit"
        /// </summary>
        public static LocalisableString HitTool => new TranslatableString(getKey(@"hit_tool"), @"Hit");

        /// <summary>
        /// "Drum roll"
        /// </summary>
        public static LocalisableString DrumRollTool => new TranslatableString(getKey(@"drum_roll_tool"), @"Drum roll");

        /// <summary>
        /// "Swell"
        /// </summary>
        public static LocalisableString SwellTool => new TranslatableString(getKey(@"swell_tool"), @"Swell");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
