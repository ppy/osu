// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class HiddenModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.HiddenMod";

        /// <summary>
        /// "Keys appear out of nowhere!"
        /// </summary>
        public static LocalisableString ManiaFadeInDescription => new TranslatableString(getKey(@"mania_fade_in_description"), "Keys appear out of nowhere!");

        /// <summary>
        /// "Coverage"
        /// </summary>
        public static LocalisableString PlayfieldCoverage => new TranslatableString(getKey(@"playfield_coverage"), "Coverage");

        /// <summary>
        /// "The proportion of playfield height that notes will be hidden for."
        /// </summary>
        public static LocalisableString PlayfieldCoverageDescription =>
            new TranslatableString(getKey(@"playfield_coverage_description"), "The proportion of playfield height that notes will be hidden for.");

        /// <summary>
        /// "Play with no approach circles and fading circles/sliders."
        /// </summary>
        public static LocalisableString OsuHiddenDescription => new TranslatableString(getKey(@"osu_hidden_description"), "Play with no approach circles and fading circles/sliders.");

        /// <summary>
        /// "Only fade approach circles"
        /// </summary>
        public static LocalisableString OnlyFadeApproachCircles => new TranslatableString(getKey(@"only_fade_approach_circles"), "Only fade approach circles");

        /// <summary>
        /// "The main body will not fade when enabled."
        /// </summary>
        public static LocalisableString OnlyFadeApproachCirclesDescription => new TranslatableString(getKey(@"only_fade_approach_circles_description"), "The main body will not fade when enabled.");

        /// <summary>
        /// "Beats fade out before you hit them!"
        /// </summary>
        public static LocalisableString TaikoHiddenDescription => new TranslatableString(getKey(@"taiko_hidden_description"), "Beats fade out before you hit them!");

        /// <summary>
        /// "Keys fade out before you hit them!"
        /// </summary>
        public static LocalisableString ManiaHiddenDescription => new TranslatableString(getKey(@"mania_hidden_description"), "Keys fade out before you hit them!");

        /// <summary>
        /// "Play with fading fruits."
        /// </summary>
        public static LocalisableString CatchHiddenDescription => new TranslatableString(getKey(@"catch_hidden_description"), "Play with fading fruits.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
