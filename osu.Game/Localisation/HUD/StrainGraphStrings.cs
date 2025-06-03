// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public class StrainGraphStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.StrainGraph";

        /// <summary>
        /// "Show background"
        /// </summary>
        public static LocalisableString ShowBackground => new TranslatableString(getKey(@"show_background"), @"Show background");

        /// <summary>
        /// "Enable background gradient"
        /// </summary>
        public static LocalisableString EnableGradient => new TranslatableString(getKey(@"enable_gradient"), @"Enable background gradient");

        /// <summary>
        /// "When enabled, the background will be filled with the specified colour in a vertical gradient style."
        /// </summary>
        public static LocalisableString EnableGradientDescription => new TranslatableString(getKey(@"enable_gradient_description"),
            @"When enabled, the background will be filled with the specified colour in a vertical gradient style.");

        /// <summary>
        /// "Use additive blending for strain graph"
        /// </summary>
        public static LocalisableString UseAdditiveBlending => new TranslatableString(getKey(@"use_additive_blending"), @"Use additive blending for strain graph");

        /// <summary>
        /// "When enabled, the additive mode would be used for graph blending. This can help make fancy visual effects, but would seem poor in some cases."
        /// </summary>
        public static LocalisableString AdditiveBlendingDescription => new TranslatableString(getKey(@"additive_blending_description"),
            @"When enabled, the additive mode would be used for graph blending. This can help make fancy visual effects, but would seem poor in some cases.");

        /// <summary>
        /// "Background colour"
        /// </summary>
        public static LocalisableString BackgroundColour => new TranslatableString(getKey(@"background_colour"), @"Background colour");

        /// <summary>
        /// "Line colour"
        /// </summary>
        public static LocalisableString LineColour => new TranslatableString(getKey(@"line_colour"), @"Line colour");

        /// <summary>
        /// "Horizontal spacing"
        /// </summary>
        public static LocalisableString HorizontalSpacing => new TranslatableString(getKey(@"horizontal_spacing"), @"Horizontal spacing");

        /// <summary>
        /// "Vertical spacing"
        /// </summary>
        public static LocalisableString VerticalSpacing => new TranslatableString(getKey(@"vertical_spacing"), @"Vertical spacing");

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

