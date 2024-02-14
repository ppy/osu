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
        /// "Cursor ripples"
        /// </summary>
        public static LocalisableString CursorRipples => new TranslatableString(getKey(@"cursor_ripples"), @"Cursor ripples");

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
        /// "Scrolling direction"
        /// </summary>
        public static LocalisableString ScrollingDirection => new TranslatableString(getKey(@"scrolling_direction"), @"Scrolling direction");

        /// <summary>
        /// "Up"
        /// </summary>
        public static LocalisableString ScrollingDirectionUp => new TranslatableString(getKey(@"scrolling_up"), @"Up");

        /// <summary>
        /// "Down"
        /// </summary>
        public static LocalisableString ScrollingDirectionDown => new TranslatableString(getKey(@"scrolling_down"), @"Down");

        /// <summary>
        /// "Scroll speed"
        /// </summary>
        public static LocalisableString ScrollSpeed => new TranslatableString(getKey(@"scroll_speed"), @"Scroll speed");

        /// <summary>
        /// "Timing-based note colouring"
        /// </summary>
        public static LocalisableString TimingBasedColouring => new TranslatableString(getKey(@"Timing_based_colouring"), @"Timing-based note colouring");

        /// <summary>
        /// "{0}ms (speed {1})"
        /// </summary>
        public static LocalisableString ScrollSpeedTooltip(int scrollTime, int scrollSpeed) => new TranslatableString(getKey(@"ruleset"), @"{0}ms (speed {1})", scrollTime, scrollSpeed);

        /// <summary>
        /// "Touch control scheme"
        /// </summary>
        public static LocalisableString TouchControlScheme => new TranslatableString(getKey(@"touch_control_scheme"), @"Touch control scheme");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
