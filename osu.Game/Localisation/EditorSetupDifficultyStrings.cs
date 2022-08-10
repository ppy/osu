// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorSetupDifficultyStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorSetupDifficulty";

        /// <summary>
        /// "The size of all hit objects"
        /// </summary>
        public static LocalisableString CircleSizeDescription => new TranslatableString(getKey(@"circle_size_description"), @"The size of all hit objects");

        /// <summary>
        /// "The rate of passive health drain throughout playable time"
        /// </summary>
        public static LocalisableString DrainRateDescription => new TranslatableString(getKey(@"drain_rate_description"), @"The rate of passive health drain throughout playable time");

        /// <summary>
        /// "The speed at which objects are presented to the player"
        /// </summary>
        public static LocalisableString ApproachRateDescription => new TranslatableString(getKey(@"approach_rate_description"), @"The speed at which objects are presented to the player");

        /// <summary>
        /// "The harshness of hit windows and difficulty of special objects (ie. spinners)"
        /// </summary>
        public static LocalisableString OverallDifficultyDescription => new TranslatableString(getKey(@"overall_difficulty_description"), @"The harshness of hit windows and difficulty of special objects (ie. spinners)");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
