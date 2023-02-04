// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class ClassicModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.ClassicMod";

        /// <summary>
        /// "Feeling nostalgic?"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "Feeling nostalgic?");

        /// <summary>
        /// "No slider head accuracy requirement"
        /// </summary>
        public static LocalisableString NoSliderHeadAccuracy => new TranslatableString(getKey(@"no_slider_head_accuracy"), "No slider head accuracy requirement");

        /// <summary>
        /// "Scores sliders proportionally to the number of ticks hit."
        /// </summary>
        public static LocalisableString NoSliderHeadAccuracyDescription => new TranslatableString(getKey(@"no_slider_head_accuracy_description"), "Scores sliders proportionally to the number of ticks hit.");

        /// <summary>
        /// "No slider head movement"
        /// </summary>
        public static LocalisableString NoSliderHeadMovement => new TranslatableString(getKey(@"no_slider_head_movement"), "No slider head movement");

        /// <summary>
        /// "Pins slider heads at their starting position, regardless of time."
        /// </summary>
        public static LocalisableString NoSliderHeadMovementDescription => new TranslatableString(getKey(@"no_slider_head_movement_description"), "Pins slider heads at their starting position, regardless of time.");

        /// <summary>
        /// "Apply classic note lock"
        /// </summary>
        public static LocalisableString ClassicNoteLock => new TranslatableString(getKey(@"classic_note_lock"), "Apply classic note lock");

        /// <summary>
        /// "Applies note lock to the full hit window."
        /// </summary>
        public static LocalisableString ClassicNoteLockDescription => new TranslatableString(getKey(@"classic_note_lock_description"), "Applies note lock to the full hit window.");

        /// <summary>
        /// "Always play a slider's trail sample"
        /// </summary>
        public static LocalisableString AlwaysPlayTailSample => new TranslatableString(getKey(@"always_play_tail_sample"), "Always play a slider's trail sample");

        /// <summary>
        /// "Always plays a slider's tail sample regardless of whether it was hit or not."
        /// </summary>
        public static LocalisableString AlwaysPlayTailSampleDescription => new TranslatableString(getKey(@"always_play_tail_sample_description"), "Always plays a slider's tail sample regardless of whether it was hit or not.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
