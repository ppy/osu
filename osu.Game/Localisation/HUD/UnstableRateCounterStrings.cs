// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class UnstableRateCounterStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.UnstableRateCounterStrings";

        /// <summary>
        /// "Convert unstable rate"
        /// </summary>
        public static LocalisableString ConvertUnstableRate => new TranslatableString(getKey(@"convert_unstable_rate"), "Convert unstable rate");

        /// <summary>
        /// "Converts unstable rate according to rate changes"
        /// </summary>
        public static LocalisableString ConvertUnstableRateDescription => new TranslatableString(getKey(@"convert_unstable_rate_description"), "Converts unstable rate according to rate changes");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
