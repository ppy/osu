// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SeasonalBackgroundModeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SeasonalBackgroundMode";

        /// <summary>
        /// "Sometimes"
        /// </summary>
        public static LocalisableString Sometimes => new TranslatableString(getKey(@"sometimes"), @"Sometimes");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}