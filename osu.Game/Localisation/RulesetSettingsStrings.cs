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
        /// "Hit object dimming"
        /// </summary>
        public static LocalisableString HitObjectDimmingStrength => new TranslatableString(getKey(@"hit_object_dimming_strength"), @"Hit object dimming");

        /// <summary>
        /// "Kiai flash strength"
        /// </summary>
        public static LocalisableString KiaiFlashStrength => new TranslatableString(getKey(@"kiai_flash_strength"), @"Kiai flash strength");

        /// <summary>
        /// "Kiai flash frequency"
        /// </summary>
        public static LocalisableString KiaiFlashFrequency => new TranslatableString(getKey(@"kiai_flash_frequency"), @"Kiai flash frequency");

        /// <summary>
        /// "1× (every beat)"
        /// </summary>
        public static LocalisableString KiaiFlashFrequency1X => new TranslatableString(getKey(@"kiai_flash_frequency_1x"), @"1× (every beat)");

        /// <summary>
        /// "0.5× (every second beat)"
        /// </summary>
        public static LocalisableString KiaiFlashFrequency05X => new TranslatableString(getKey(@"kiai_flash_frequency_0_5x"), @"0.5× (every second beat)");

        /// <summary>
        /// "0.25× (every fourth beat)"
        /// </summary>
        public static LocalisableString KiaiFlashFrequency025X => new TranslatableString(getKey(@"kiai_flash_frequency_0_25x"), @"0.25× (every fourth beat)");

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
        /// "{0}ms (speed {1:N1})"
        /// </summary>
        public static LocalisableString ScrollSpeedTooltip(int scrollTime, double scrollSpeed) => new TranslatableString(getKey(@"ruleset"), @"{0}ms (speed {1:N1})", scrollTime, scrollSpeed);

        /// <summary>
        /// "Touch control scheme"
        /// </summary>
        public static LocalisableString TouchControlScheme => new TranslatableString(getKey(@"touch_control_scheme"), @"Touch control scheme");

        /// <summary>
        /// "Mobile layout"
        /// </summary>
        public static LocalisableString MobileLayout => new TranslatableString(getKey(@"mobile_layout"), @"Mobile layout");

        /// <summary>
        /// "Portrait"
        /// </summary>
        public static LocalisableString Portrait => new TranslatableString(getKey(@"portrait"), @"Portrait");

        /// <summary>
        /// "Landscape"
        /// </summary>
        public static LocalisableString Landscape => new TranslatableString(getKey(@"landscape"), @"Landscape");

        /// <summary>
        /// "Landscape (expanded columns)"
        /// </summary>
        public static LocalisableString LandscapeExpandedColumns => new TranslatableString(getKey(@"landscape_expanded_columns"), @"Landscape (expanded columns)");

        /// <summary>
        /// "Touch overlay"
        /// </summary>
        public static LocalisableString TouchOverlay => new TranslatableString(getKey(@"touch_overlay"), @"Touch overlay");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
