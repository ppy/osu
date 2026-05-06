// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ModSelectOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ModSelectOverlay";

        /// <summary>
        /// "Mod Select"
        /// </summary>
        public static LocalisableString ModSelectTitle => new TranslatableString(getKey(@"mod_select_title"), @"Mod Select");

        /// <summary>
        /// "{0} mods"
        /// </summary>
        public static LocalisableString Mods(int count) => new TranslatableString(getKey(@"mods"), @"{0} mods", count);

        /// <summary>
        /// "all mods"
        /// </summary>
        public static LocalisableString AllMods => new TranslatableString(getKey(@"all_mods"), @"all mods");

        /// <summary>
        /// "Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play. Others are just for fun."
        /// </summary>
        public static LocalisableString ModSelectDescription => new TranslatableString(getKey(@"mod_select_description"),
            @"Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play. Others are just for fun.");

        /// <summary>
        /// "Mod Customisation"
        /// </summary>
        public static LocalisableString ModCustomisation => new TranslatableString(getKey(@"mod_customisation"), @"Mod Customisation");

        /// <summary>
        /// "Personal Presets"
        /// </summary>
        public static LocalisableString PersonalPresets => new TranslatableString(getKey(@"personal_presets"), @"Personal Presets");

        /// <summary>
        /// "Add preset"
        /// </summary>
        public static LocalisableString AddPreset => new TranslatableString(getKey(@"add_preset"), @"Add preset");

        /// <summary>
        /// "Use current mods"
        /// </summary>
        public static LocalisableString UseCurrentMods => new TranslatableString(getKey(@"use_current_mods"), @"Use current mods");

        /// <summary>
        /// "tab to search..."
        /// </summary>
        public static LocalisableString TabToSearch => new TranslatableString(getKey(@"tab_to_search"), @"tab to search...");

        /// <summary>
        /// "Score Multiplier"
        /// </summary>
        public static LocalisableString ScoreMultiplier => new TranslatableString(getKey(@"score_multiplier"), @"Score Multiplier");

        /// <summary>
        /// "Ranked"
        /// </summary>
        public static LocalisableString Ranked => new TranslatableString(getKey(@"ranked"), @"Ranked");

        /// <summary>
        /// "Performance points can be granted for the active mods."
        /// </summary>
        public static LocalisableString RankedExplanation => new TranslatableString(getKey(@"ranked_explanation"), @"Performance points can be granted for the active mods.");

        /// <summary>
        /// "Unranked"
        /// </summary>
        public static LocalisableString Unranked => new TranslatableString(getKey(@"unranked"), @"Unranked");

        /// <summary>
        /// "Performance points will not be granted due to active mods."
        /// </summary>
        public static LocalisableString UnrankedExplanation => new TranslatableString(getKey(@"unranked_explanation"), @"Performance points will not be granted due to active mods.");

        /// <summary>
        /// "Customise"
        /// </summary>
        public static LocalisableString CustomisationPanelHeader => new TranslatableString(getKey(@"customisation_panel_header"), @"Customise");

        /// <summary>
        /// "No mod selected which can be customised."
        /// </summary>
        public static LocalisableString CustomisationPanelDisabledReason => new TranslatableString(getKey(@"customisation_panel_disabled_reason"), @"No mod selected which can be customised.");

        /// <summary>
        /// "Fail if your accuracy drops too low!"
        /// </summary>
        public static LocalisableString ModAccuracyChallengeDescription => new TranslatableString(getKey(@"mod_accuracy_challenge_description"),
            @"Fail if your accuracy drops too low!");

        /// <summary>
        /// "Let track speed adapt to you."
        /// </summary>
        public static LocalisableString ModAdaptativeSpeedDescription => new TranslatableString(getKey(@"mod_adaptative_speed_description"),
            @"Let track speed adapt to you.");

        /// <summary>
        /// "Watch a perfect automated play through the song."
        /// </summary>
        public static LocalisableString ModAutoplayDescription => new TranslatableString(getKey(@"mod_autoplay_description"),
            @"Watch a perfect automated play through the song.");

        /// <summary>
        /// "The whole playfield is on a wheel!"
        /// </summary>
        public static LocalisableString ModBarrelRollDescription => new TranslatableString(getKey(@"mod_barrel_roll_description"),
            @"The whole playfield is on a wheel!");

        /// <summary>
        /// "Watch the video without visual distractions."
        /// </summary>
        public static LocalisableString ModCinemaDescription => new TranslatableString(getKey(@"mod_cinema_description"),
            @"Watch the video without visual distractions.");

        /// <summary>
        /// "Feeling nostalgic?"
        /// </summary>
        public static LocalisableString ModClassicDescription => new TranslatableString(getKey(@"mod_classic_description"),
            @"Feeling nostalgic?");

        /// <summary>
        /// "Whoaaaaa..."
        /// </summary>
        public static LocalisableString ModDaycoreDescription => new TranslatableString(getKey(@"mod_daycore_description"),
            @"Whoaaaaa...");

        /// <summary>
        /// "Override a beatmap's difficulty settings."
        /// </summary>
        public static LocalisableString ModDifficultyAdjustDescription => new TranslatableString(getKey(@"mod_difficulty_adjust_description"),
            @"Override a beatmap's difficulty settings.");

        /// <summary>
        /// "Zoooooooooom..."
        /// </summary>
        public static LocalisableString ModDoubleTimeDescription => new TranslatableString(getKey(@"mod_double_time_description"),
            @"Zoooooooooom...");

        /// <summary>
        /// "Restricted view area."
        /// </summary>
        public static LocalisableString ModFlashlightDescription => new TranslatableString(getKey(@"mod_flashlight_description"),
            @"Restricted view area.");

        /// <summary>
        /// "Less zoom..."
        /// </summary>
        public static LocalisableString ModHalfTimeDescription => new TranslatableString(getKey(@"mod_half_time_description"),
            @"Less zoom...");

        /// <summary>
        /// "Everything just got a bit harder..."
        /// </summary>
        public static LocalisableString ModHardRockDescription => new TranslatableString(getKey(@"mod_hard_rock_description"),
            @"Everything just got a bit harder...");

        /// <summary>
        /// "Can you still feel the rhythm without music?"
        /// </summary>
        public static LocalisableString ModMutedDescription => new TranslatableString(getKey(@"mod_muted_description"),
            @"Can you still feel the rhythm without music?");

        /// <summary>
        /// "Uguuuuuuuu..."
        /// </summary>
        public static LocalisableString ModNightcoreDescription => new TranslatableString(getKey(@"mod_nightcore_description"),
            @"Uguuuuuuuu...");

        /// <summary>
        /// "You can't fail, no matter what."
        /// </summary>
        public static LocalisableString ModNoFailDescription => new TranslatableString(getKey(@"mod_no_fail_description"),
            @"You can't fail, no matter what.");

        /// <summary>
        /// "No mods applied."
        /// </summary>
        public static LocalisableString ModNoModDescription => new TranslatableString(getKey(@"mod_no_mod_description"),
            @"No mods applied.");

        /// <summary>
        /// "SS or quit."
        /// </summary>
        public static LocalisableString ModPerfectDescription => new TranslatableString(getKey(@"mod_perfect_description"),
            @"SS or quit.");

        /// <summary>
        /// "Score set on earlier osu! versions with the V2 scoring algorithm active."
        /// </summary>
        public static LocalisableString ModScoreV2Description => new TranslatableString(getKey(@"mod_score_v2_description"),
            @"Score set on earlier osu! versions with the V2 scoring algorithm active.");

        /// <summary>
        /// "Miss and fail."
        /// </summary>
        public static LocalisableString ModSuddenDeathDescription => new TranslatableString(getKey(@"mod_sudden_death_description"),
            @"Miss and fail.");

        /// <summary>
        /// "Colours hit objects based on the rhythm."
        /// </summary>
        public static LocalisableString ModSynesthesiaDescription => new TranslatableString(getKey(@"mod_synesthesia_description"),
            @"Colours hit objects based on the rhythm.");

        /// <summary>
        /// "Automatically applied to plays on devices with a touchscreen."
        /// </summary>
        public static LocalisableString ModTouchDeviceDescription => new TranslatableString(getKey(@"mod_touch_device_description"),
            @"Automatically applied to plays on devices with a touchscreen.");

        /// <summary>
        /// "Sloooow doooown..."
        /// </summary>
        public static LocalisableString ModWindDownDescription => new TranslatableString(getKey(@"mod_wind_down_description"),
            @"Sloooow doooown...");

        /// <summary>
        /// "Can you keep up?"
        /// </summary>
        public static LocalisableString ModWindUpDescription => new TranslatableString(getKey(@"mod_wind_up_description"),
            @"Can you keep up?");

        /// <summary>
        /// "This mod could not be resolved by the game."
        /// </summary>
        public static LocalisableString UnknownModDescription => new TranslatableString(getKey(@"unknown_mod_description"),
            @"This mod could not be resolved by the game.");

        /// <summary>
        /// "Don't use the same key twice in a row!"
        /// </summary>
        public static LocalisableString OsuModAlternateDescription => new TranslatableString(getKey(@"osu_mod_alternate_description"),
            @"Don't use the same key twice in a row!");

        /// <summary>
        /// "Never trust the approach circles..."
        /// </summary>
        public static LocalisableString OsuModApproachDifferentDescription => new TranslatableString(getKey(@"osu_mod_approach_different_description"),
            @"Never trust the approach circles...");

        /// <summary>
        /// "Automatic cursor movement - just follow the rhythm."
        /// </summary>
        public static LocalisableString OsuModAutopilotDescription => new TranslatableString(getKey(@"osu_mod_autopilot_description"),
            @"Automatic cursor movement - just follow the rhythm.");

        /// <summary>
        /// "Play with blinds on your screen."
        /// </summary>
        public static LocalisableString OsuModBlindsDescription => new TranslatableString(getKey(@"osu_mod_blinds_description"),
            @"Play with blinds on your screen.");

        /// <summary>
        /// "The cursor blooms into.. a larger cursor!"
        /// </summary>
        public static LocalisableString OsuModBloomDescription => new TranslatableString(getKey(@"osu_mod_bloom_description"),
            @"The cursor blooms into.. a larger cursor!");

        /// <summary>
        /// "Don't let their popping distract you!"
        /// </summary>
        public static LocalisableString OsuModBubblesDescription => new TranslatableString(getKey(@"osu_mod_bubbles_description"),
            @"Don't let their popping distract you!");

        /// <summary>
        /// "Hit them at the right size!"
        /// </summary>
        public static LocalisableString OsuModDeflateDescription => new TranslatableString(getKey(@"osu_mod_deflate_description"),
            @"Hit them at the right size!");

        /// <summary>
        /// "3D. Almost."
        /// </summary>
        public static LocalisableString OsuModDepthDescription => new TranslatableString(getKey(@"osu_mod_depth_description"),
            @"3D. Almost.");

        /// <summary>
        /// "Larger circles, more forgiving HP drain, less accuracy required, and extra lives!"
        /// </summary>
        public static LocalisableString OsuModEasyDescription => new TranslatableString(getKey(@"osu_mod_easy_description"),
            @"Larger circles, more forgiving HP drain, less accuracy required, and extra lives!");

        /// <summary>
        /// "Burn the notes into your memory."
        /// </summary>
        public static LocalisableString OsuModFreezeFrameDescription => new TranslatableString(getKey(@"osu_mod_freeze_frame_description"),
            @"Burn the notes into your memory.");

        /// <summary>
        /// "Hit them at the right size!"
        /// </summary>
        public static LocalisableString OsuModGrowDescription => new TranslatableString(getKey(@"osu_mod_grow_description"),
            @"Hit them at the right size!");

        /// <summary>
        /// "Play with no approach circles and fading circles/sliders."
        /// </summary>
        public static LocalisableString OsuModHiddenDescription => new TranslatableString(getKey(@"osu_mod_hidden_description"),
            @"Play with no approach circles and fading circles/sliders.");

        /// <summary>
        /// "No need to chase the circles – your cursor is a magnet!"
        /// </summary>
        public static LocalisableString OsuModMagnetisedDescription => new TranslatableString(getKey(@"osu_mod_magnetised_description"),
            @"No need to chase the circles – your cursor is a magnet!");

        /// <summary>
        /// "Flip objects on the chosen axes."
        /// </summary>
        public static LocalisableString OsuModMirrorDescription => new TranslatableString(getKey(@"osu_mod_mirror_description"),
            @"Flip objects on the chosen axes.");

        /// <summary>
        /// "Where's the cursor?"
        /// </summary>
        public static LocalisableString OsuModNoScopeDescription => new TranslatableString(getKey(@"osu_mod_no_scope_description"),
            @"Where's the cursor?");

        /// <summary>
        /// "It never gets boring!"
        /// </summary>
        public static LocalisableString OsuModRandomDescription => new TranslatableString(getKey(@"osu_mod_random_description"),
            @"It never gets boring!");

        /// <summary>
        /// "You don't need to click. Give your clicking/tapping fingers a break from the heat of things."
        /// </summary>
        public static LocalisableString OsuModRelaxDescription => new TranslatableString(getKey(@"osu_mod_relax_description"),
            @"You don't need to click. Give your clicking/tapping fingers a break from the heat of things.");

        /// <summary>
        /// "Hit objects run away!"
        /// </summary>
        public static LocalisableString OsuModRepelDescription => new TranslatableString(getKey(@"osu_mod_repel_description"),
            @"Hit objects run away!");

        /// <summary>
        /// "You must only use one key!"
        /// </summary>
        public static LocalisableString OsuModSingleTapDescription => new TranslatableString(getKey(@"osu_mod_single_tap_description"),
            @"You must only use one key!");

        /// <summary>
        /// "Circles spin in. No approach circles."
        /// </summary>
        public static LocalisableString OsuModSpinInDescription => new TranslatableString(getKey(@"osu_mod_spin_in_description"),
            @"Circles spin in. No approach circles.");

        /// <summary>
        /// "Spinners will be automatically completed."
        /// </summary>
        public static LocalisableString OsuModSpunOutDescription => new TranslatableString(getKey(@"osu_mod_spun_out_description"),
            @"Spinners will be automatically completed.");

        /// <summary>
        /// "Once you start a slider, follow precisely or get a miss."
        /// </summary>
        public static LocalisableString OsuModStrictTrackingDescription => new TranslatableString(getKey(@"osu_mod_strict_tracking_description"),
            @"Once you start a slider, follow precisely or get a miss.");

        /// <summary>
        /// "Practice keeping up with the beat of the song."
        /// </summary>
        public static LocalisableString OsuModTargetPracticeDescription => new TranslatableString(getKey(@"osu_mod_target_practice_description"),
            @"Practice keeping up with the beat of the song.");

        /// <summary>
        /// "Put your faith in the approach circles..."
        /// </summary>
        public static LocalisableString OsuModTraceableDescription => new TranslatableString(getKey(@"osu_mod_traceable_description"),
            @"Put your faith in the approach circles...");

        /// <summary>
        /// "Everything rotates. EVERYTHING."
        /// </summary>
        public static LocalisableString OsuModTransformDescription => new TranslatableString(getKey(@"osu_mod_tranform_description"),
            @"Everything rotates. EVERYTHING.");

        /// <summary>
        /// "They just won't stay still..."
        /// </summary>
        public static LocalisableString OsuModWiggleDescription => new TranslatableString(getKey(@"osu_mod_wiggle_description"),
            @"They just won't stay still...");


        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
