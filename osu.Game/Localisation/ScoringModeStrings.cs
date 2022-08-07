// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ScoringModeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ScoringMode";

        /// <summary>
        /// "Standardised"
        /// </summary>
        public static LocalisableString Standardised => new TranslatableString(getKey(@"standardised"), @"Standardised");

        /// <summary>
        /// "Classic"
        /// </summary>
        public static LocalisableString Classic => new TranslatableString(getKey(@"classic"), @"Classic");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}