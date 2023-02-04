// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class TargetPracticeModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.TargetPracticeMod";

        /// <summary>
        /// "Seed"
        /// </summary>
        public static LocalisableString Seed => new TranslatableString(getKey(@"seed"), "Seed");

        /// <summary>
        /// "Use a custom seed instead of a random one"
        /// </summary>
        public static LocalisableString SeedDescription => new TranslatableString(getKey(@"seed_description"), "Use a custom seed instead of a random one");

        /// <summary>
        /// "Practice keeping up with the beat of the song."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"target_practice_description"), "Practice keeping up with the beat of the song.");

        /// <summary>
        /// "Metronome ticks"
        /// </summary>
        public static LocalisableString Metronome => new TranslatableString(getKey(@"metronome"), "Metronome ticks");

        /// <summary>
        /// "Whether a metronome beat should play in the background"
        /// </summary>
        public static LocalisableString MetronomeDescription =>
            new TranslatableString(getKey(@"metronome_description"), "Whether a metronome beat should play in the background");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
