// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorSetupStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorSetup";

        /// <summary>
        /// "beatmap setup"
        /// </summary>
        public static LocalisableString BeatmapSetup => new TranslatableString(getKey(@"beatmap_setup"), @"beatmap setup");

        /// <summary>
        /// "change general settings of your beatmap"
        /// </summary>
        public static LocalisableString BeatmapSetupDescription => new TranslatableString(getKey(@"beatmap_setup_description"), @"change general settings of your beatmap");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
