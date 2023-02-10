// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class RulesetSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RulesetSettings";

        /// <summary>
        /// "Rulesets"
        /// </summary>
        public static LocalisableString Rulesets => new TranslatableString(getKey(@"rulesets"), @"Rulesets");

        /// <summary>
        /// "Snaking in sliders"
        /// </summary>
        public static LocalisableString SnakingInSliders => new TranslatableString(getKey(@"snaking_in_sliders"), @"Snaking in sliders");

        /// <summary>
        /// "Snaking out sliders"
        /// </summary>
        public static LocalisableString SnakingOutSliders => new TranslatableString(getKey(@"snaking_out_sliders"), @"Snaking out sliders");

        /// <summary>
        /// "Cursor trail"
        /// </summary>
        public static LocalisableString CursorTrail => new TranslatableString(getKey(@"cursor_trail"), @"Cursor trail");

        /// <summary>
        /// "Playfield border style"
        /// </summary>
        public static LocalisableString PlayfieldBorderStyle => new TranslatableString(getKey(@"playfield_border_style"), @"Playfield border style");

        /// <summary>
        /// "None"
        /// </summary>
        public static LocalisableString BorderNone => new TranslatableString(getKey(@"no_borders"), @"None");

        /// <summary>
        /// "Corners"
        /// </summary>
        public static LocalisableString BorderCorners => new TranslatableString(getKey(@"corner_borders"), @"Corners");

        /// <summary>
        /// "Full"
        /// </summary>
        public static LocalisableString BorderFull => new TranslatableString(getKey(@"full_borders"), @"Full");

        /// <summary>
        /// "滚动方向"
        /// </summary>
        public static LocalisableString ScrollingDirection => new TranslatableString(getKey(@"llin_scrolling_direction"), @"滚动方向");

        /// <summary>
        /// "从下往上"
        /// </summary>
        public static LocalisableString ScrollingDirectionUp => new TranslatableString(getKey(@"llin_scrolling_up"), @"从下往上");

        /// <summary>
        /// "从上往下"
        /// </summary>
        public static LocalisableString ScrollingDirectionDown => new TranslatableString(getKey(@"llin_scrolling_down"), @"从上往下");

        /// <summary>
        /// "滚动速度"
        /// </summary>
        public static LocalisableString ScrollSpeed => new TranslatableString(getKey(@"llin_scroll_speed"), @"滚动速度");

        /// <summary>
        /// "基于Timing的Note颜色"
        /// </summary>
        public static LocalisableString TimingBasedColouring => new TranslatableString(getKey(@"llin_Timing_based_colouring"), @"基于Timing的Note颜色");

        /// <summary>
        /// "{0}ms (speed {1})"
        /// </summary>
        public static LocalisableString ScrollSpeedTooltip(double arg0, int arg1) => new TranslatableString(getKey(@"ruleset"), @"{0}ms (speed {1})", arg0, arg1);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
