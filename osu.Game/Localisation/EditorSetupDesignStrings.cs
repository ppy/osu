// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorSetupDesignStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorSetupDesign";

        /// <summary>
        /// "Design"
        /// </summary>
        public static LocalisableString Design => new TranslatableString(getKey(@"design"), @"Design");

        /// <summary>
        /// "Enable countdown"
        /// </summary>
        public static LocalisableString EnableCountdown => new TranslatableString(getKey(@"enable_countdown"), @"Enable countdown");

        /// <summary>
        /// "If enabled, an &quot;Are you ready? 3, 2, 1, GO!&quot; countdown will be inserted at the beginning of the beatmap, assuming there is enough time to do so."
        /// </summary>
        public static LocalisableString CountdownDescription => new TranslatableString(getKey(@"countdown_description"), @"If enabled, an ""Are you ready? 3, 2, 1, GO!"" countdown will be inserted at the beginning of the beatmap, assuming there is enough time to do so.");

        /// <summary>
        /// "Countdown speed"
        /// </summary>
        public static LocalisableString CountdownSpeed => new TranslatableString(getKey(@"countdown_speed"), @"Countdown speed");

        /// <summary>
        /// "If the countdown sounds off-time, use this to make it appear one or more beats early."
        /// </summary>
        public static LocalisableString CountdownOffsetDescription => new TranslatableString(getKey(@"countdown_offset_description"), @"If the countdown sounds off-time, use this to make it appear one or more beats early.");

        /// <summary>
        /// "Countdown offset"
        /// </summary>
        public static LocalisableString CountdownOffset => new TranslatableString(getKey(@"countdown_offset"), @"Countdown offset");

        /// <summary>
        /// "Widescreen support"
        /// </summary>
        public static LocalisableString WidescreenSupport => new TranslatableString(getKey(@"widescreen_support"), @"Widescreen support");

        /// <summary>
        /// "Allows storyboards to use the full screen space, rather than be confined to a 4:3 area."
        /// </summary>
        public static LocalisableString WidescreenSupportDescription => new TranslatableString(getKey(@"widescreen_support_description"), @"Allows storyboards to use the full screen space, rather than be confined to a 4:3 area.");

        /// <summary>
        /// "Epilepsy warning"
        /// </summary>
        public static LocalisableString EpilepsyWarning => new TranslatableString(getKey(@"epilepsy_warning"), @"Epilepsy warning");

        /// <summary>
        /// "Recommended if the storyboard or video contain scenes with rapidly flashing colours."
        /// </summary>
        public static LocalisableString EpilepsyWarningDescription => new TranslatableString(getKey(@"epilepsy_warning_description"), @"Recommended if the storyboard or video contain scenes with rapidly flashing colours.");

        /// <summary>
        /// "Letterbox during breaks"
        /// </summary>
        public static LocalisableString LetterboxDuringBreaks => new TranslatableString(getKey(@"letterbox_during_breaks"), @"Letterbox during breaks");

        /// <summary>
        /// "Adds horizontal letterboxing to give a cinematic look during breaks."
        /// </summary>
        public static LocalisableString LetterboxDuringBreaksDescription => new TranslatableString(getKey(@"letterbox_during_breaks_description"), @"Adds horizontal letterboxing to give a cinematic look during breaks.");

        /// <summary>
        /// "Samples match playback rate"
        /// </summary>
        public static LocalisableString SamplesMatchPlaybackRate => new TranslatableString(getKey(@"samples_match_playback_rate"), @"Samples match playback rate");

        /// <summary>
        /// "When enabled, all samples will speed up or slow down when rate-changing mods are enabled."
        /// </summary>
        public static LocalisableString SamplesMatchPlaybackRateDescription => new TranslatableString(getKey(@"samples_match_playback_rate_description"), @"When enabled, all samples will speed up or slow down when rate-changing mods are enabled.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
