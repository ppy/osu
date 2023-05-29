// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorSetupStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorSetup";

        /// <summary>
        /// "Beatmap Setup"
        /// </summary>
        public static LocalisableString BeatmapSetup => new TranslatableString(getKey(@"beatmap_setup"), @"Beatmap Setup");

        /// <summary>
        /// "change general settings of your beatmap"
        /// </summary>
        public static LocalisableString BeatmapSetupDescription => new TranslatableString(getKey(@"beatmap_setup_description"), @"change general settings of your beatmap");

        /// <summary>
        /// "Colours"
        /// </summary>
        public static LocalisableString ColoursHeader => new TranslatableString(getKey(@"colours_header"), @"Colours");

        /// <summary>
        /// "Hit circle / Slider Combos"
        /// </summary>
        public static LocalisableString HitCircleSliderCombos => new TranslatableString(getKey(@"hit_circle_slider_combos"), @"Hit circle / Slider Combos");

        /// <summary>
        /// "Design"
        /// </summary>
        public static LocalisableString DesignHeader => new TranslatableString(getKey(@"design_header"), @"Design");

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

        /// <summary>
        /// "The size of all hit objects"
        /// </summary>
        public static LocalisableString CircleSizeDescription => new TranslatableString(getKey(@"circle_size_description"), @"The size of all hit objects");

        /// <summary>
        /// "The rate of passive health drain throughout playable time"
        /// </summary>
        public static LocalisableString DrainRateDescription => new TranslatableString(getKey(@"drain_rate_description"), @"The rate of passive health drain throughout playable time");

        /// <summary>
        /// "The speed at which objects are presented to the player"
        /// </summary>
        public static LocalisableString ApproachRateDescription => new TranslatableString(getKey(@"approach_rate_description"), @"The speed at which objects are presented to the player");

        /// <summary>
        /// "The harshness of hit windows and difficulty of special objects (ie. spinners)"
        /// </summary>
        public static LocalisableString OverallDifficultyDescription => new TranslatableString(getKey(@"overall_difficulty_description"), @"The harshness of hit windows and difficulty of special objects (ie. spinners)");

        /// <summary>
        /// "Tick Rate"
        /// </summary>
        public static LocalisableString TickRate => new TranslatableString(getKey(@"tick_rate"), @"Tick Rate");

        /// <summary>
        /// "Determines how many &quot;ticks&quot; are generated within long hit objects. A tick rate of 1 will generate ticks on each beat, 2 would be twice per beat, etc."
        /// </summary>
        public static LocalisableString TickRateDescription => new TranslatableString(getKey(@"tick_rate_description"), @"Determines how many ""ticks"" are generated within long hit objects. A tick rate of 1 will generate ticks on each beat, 2 would be twice per beat, etc.");

        /// <summary>
        /// "Base Velocity"
        /// </summary>
        public static LocalisableString BaseVelocity => new TranslatableString(getKey(@"base_velocity"), @"Base Velocity");

        /// <summary>
        /// "The base velocity of the beatmap, affecting things like slider velocity and scroll speed in some rulesets."
        /// </summary>
        public static LocalisableString BaseVelocityDescription => new TranslatableString(getKey(@"base_velocity_description"), @"The base velocity of the beatmap, affecting things like slider velocity and scroll speed in some rulesets.");

        /// <summary>
        /// "Metadata"
        /// </summary>
        public static LocalisableString MetadataHeader => new TranslatableString(getKey(@"metadata_header"), @"Metadata");

        /// <summary>
        /// "Romanised Artist"
        /// </summary>
        public static LocalisableString RomanisedArtist => new TranslatableString(getKey(@"romanised_artist"), @"Romanised Artist");

        /// <summary>
        /// "Romanised Title"
        /// </summary>
        public static LocalisableString RomanisedTitle => new TranslatableString(getKey(@"romanised_title"), @"Romanised Title");

        /// <summary>
        /// "Creator"
        /// </summary>
        public static LocalisableString Creator => new TranslatableString(getKey(@"creator"), @"Creator");

        /// <summary>
        /// "Difficulty Name"
        /// </summary>
        public static LocalisableString DifficultyName => new TranslatableString(getKey(@"difficulty_name"), @"Difficulty Name");

        /// <summary>
        /// "Resources"
        /// </summary>
        public static LocalisableString ResourcesHeader => new TranslatableString(getKey(@"resources_header"), @"Resources");

        /// <summary>
        /// "Audio Track"
        /// </summary>
        public static LocalisableString AudioTrack => new TranslatableString(getKey(@"audio_track"), @"Audio Track");

        /// <summary>
        /// "Click to select a track"
        /// </summary>
        public static LocalisableString ClickToSelectTrack => new TranslatableString(getKey(@"click_to_select_track"), @"Click to select a track");

        /// <summary>
        /// "Click to replace the track"
        /// </summary>
        public static LocalisableString ClickToReplaceTrack => new TranslatableString(getKey(@"click_to_replace_track"), @"Click to replace the track");

        /// <summary>
        /// "Click to select a background image"
        /// </summary>
        public static LocalisableString ClickToSelectBackground => new TranslatableString(getKey(@"click_to_select_background"), @"Click to select a background image");

        /// <summary>
        /// "Click to replace the background image"
        /// </summary>
        public static LocalisableString ClickToReplaceBackground => new TranslatableString(getKey(@"click_to_replace_background"), @"Click to replace the background image");

        /// <summary>
        /// "Ruleset ({0})"
        /// </summary>
        public static LocalisableString RulesetHeader(string arg0) => new TranslatableString(getKey(@"ruleset"), @"Ruleset ({0})", arg0);

        /// <summary>
        /// "Combo"
        /// </summary>
        public static LocalisableString ComboColourPrefix => new TranslatableString(getKey(@"combo_colour_prefix"), @"Combo");

        /// <summary>
        /// "Artist"
        /// </summary>
        public static LocalisableString Artist => new TranslatableString(getKey(@"artist"), @"Artist");

        /// <summary>
        /// "Title"
        /// </summary>
        public static LocalisableString Title => new TranslatableString(getKey(@"title"), @"Title");

        /// <summary>
        /// "Difficulty"
        /// </summary>
        public static LocalisableString DifficultyHeader => new TranslatableString(getKey(@"difficulty_header"), @"Difficulty");

        /// <summary>
        /// "Drag image here to set beatmap background!"
        /// </summary>
        public static LocalisableString DragToSetBackground => new TranslatableString(getKey(@"drag_to_set_background"), @"Drag image here to set beatmap background!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
