// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class OsuSettingsSubsectionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.OsuSettingsSubsection";

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

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
