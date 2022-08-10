// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorSetupColoursStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorSetupColours";

        /// <summary>
        /// "Colours"
        /// </summary>
        public static LocalisableString Colours => new TranslatableString(getKey(@"colours"), @"Colours");

        /// <summary>
        /// "Hitcircle / Slider Combos"
        /// </summary>
        public static LocalisableString HitcircleSliderCombos => new TranslatableString(getKey(@"hitcircle_slider_combos"), @"Hitcircle / Slider Combos");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
