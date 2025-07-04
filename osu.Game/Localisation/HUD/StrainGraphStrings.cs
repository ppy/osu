// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public class StrainGraphStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.StrainGraph";

        /// <summary>
        /// "Line colour"
        /// </summary>
        public static LocalisableString LineColour => new TranslatableString(getKey(@"line_colour"), @"Line colour");

        /// <summary>
        /// "Section granularity"
        /// </summary>
        public static LocalisableString SectionGranularity => new TranslatableString(getKey(@"section_granularity"), @"Section granularity");

        /// <summary>
        /// "The number of sections the beatmap should be divided into. High values make the graph more detailed and wider."
        /// </summary>
        public static LocalisableString SectionGranularityDescription => new TranslatableString(getKey(@"section_granularity_description"), @"The number of sections the beatmap should be divided into. High values make the graph more detailed and wider.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

