// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class BeatmapLeaderboardWedgeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BeatmapLeaderboardWedge";

        /// <summary>
        /// "Scope"
        /// </summary>
        public static LocalisableString Scope => new TranslatableString(getKey(@"scope"), @"Scope");

        /// <summary>
        /// "Local"
        /// </summary>
        public static LocalisableString Local => new TranslatableString(getKey(@"local"), @"Local");

        /// <summary>
        /// "Global"
        /// </summary>
        public static LocalisableString Global => new TranslatableString(getKey(@"global"), @"Global");

        /// <summary>
        /// "Country"
        /// </summary>
        public static LocalisableString Country => new TranslatableString(getKey(@"country"), @"Country");

        /// <summary>
        /// "Friend"
        /// </summary>
        public static LocalisableString Friend => new TranslatableString(getKey(@"friend"), @"Friend");

        /// <summary>
        /// "Team"
        /// </summary>
        public static LocalisableString Team => new TranslatableString(getKey(@"team"), @"Team");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
