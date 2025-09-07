// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ModPitchUpStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ModPitchUp";

        /// <summary>
        /// "Pitch Up"
        /// </summary>
        public static LocalisableString PitchUp => new TranslatableString(getKey(@"pitch_up"), @"Pitch Up");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}