// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class MutedModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.MutedMod";

        /// <summary>
        /// "Can you still feel the rhythm without music?"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "Can you still feel the rhythm without music?");

        /// <summary>
        /// "Start muted"
        /// </summary>
        public static LocalisableString InverseMuting => new TranslatableString(getKey(@"inverse_muting"), "Start muted");

        /// <summary>
        /// "Increase volume as combo builds."
        /// </summary>
        public static LocalisableString InverseMutingDescription => new TranslatableString(getKey(@"inverse_muting_description"), "Increase volume as combo builds.");

        /// <summary>
        /// "Enable metronome"
        /// </summary>
        public static LocalisableString EnableMetronome => new TranslatableString(getKey(@"enable_metronome"), "Enable metronome");

        /// <summary>
        /// "Add a metronome beat to help you keep track of the rhythm."
        /// </summary>
        public static LocalisableString EnableMetronomeDescription => new TranslatableString(getKey(@"enable_metronome"), "Add a metronome beat to help you keep track of the rhythm.");

        /// <summary>
        /// "Final volume at combo"
        /// </summary>
        public static LocalisableString MuteComboCount => new TranslatableString(getKey(@"mute_combo_count"), "Final volume at combo");

        /// <summary>
        /// "The combo count at which point the track reaches its final volume."
        /// </summary>
        public static LocalisableString MuteComboCountDescription => new TranslatableString(getKey(@"mute_combo_count_description"), "The combo count at which point the track reaches its final volume.");

        /// <summary>
        /// "Mute hit sounds"
        /// </summary>
        public static LocalisableString AffectsHitSounds => new TranslatableString(getKey(@"affects_hit_sounds"), "Mute hit sounds");

        /// <summary>
        /// "Hit sounds are also muted alongside the track."
        /// </summary>
        public static LocalisableString AffectsHitSoundsDescription => new TranslatableString(getKey(@"affects_hit_sounds_description"), "Hit sounds are also muted alongside the track.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
