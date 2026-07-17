// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Catch
{
    public static class CatchRulesetStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Catch.CatchRuleset";

        /// <summary>
        /// "Affects the size of fruits."
        /// </summary>
        public static LocalisableString CircleSizeDescription => new TranslatableString(getKey(@"circle_size_description"), @"Affects the size of fruits.");

        /// <summary>
        /// "Affects how early fruits fade in on the screen."
        /// </summary>
        public static LocalisableString ApproachRateDescription => new TranslatableString(getKey(@"approach_rate_description"), @"Affects how early fruits fade in on the screen.");

        /// <summary>
        /// "Fade-in time"
        /// </summary>
        public static LocalisableString FadeInTime => new TranslatableString(getKey(@"fade_in_time"), @"Fade-in time");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
