// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ManiaSettingsSubsectionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ManiaSettingsSubsection";

        /// <summary>
        /// "Scrolling direction"
        /// </summary>
        public static LocalisableString ScrollingDirection => new TranslatableString(getKey(@"scrolling_direction"), @"Scrolling direction");

        /// <summary>
        /// "Scroll speed"
        /// </summary>
        public static LocalisableString ScrollSpeed => new TranslatableString(getKey(@"scroll_speed"), @"Scroll speed");

        /// <summary>
        /// "Timing-based note colouring"
        /// </summary>
        public static LocalisableString TimingBasedNoteColouring => new TranslatableString(getKey(@"timing_based_note_colouring"), @"Timing-based note colouring");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
