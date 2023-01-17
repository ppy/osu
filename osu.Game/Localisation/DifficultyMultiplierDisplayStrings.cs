// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DifficultyMultiplierDisplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DifficultyMultiplierDisplay";

        /// <summary>
        /// "Difficulty Multiplier"
        /// </summary>
        public static LocalisableString DifficultyMultiplier => new TranslatableString(getKey(@"difficulty_multiplier"), @"Difficulty Multiplier");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
