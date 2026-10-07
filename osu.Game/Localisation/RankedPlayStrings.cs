// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class RankedPlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RankedPlay";

        /// <summary>
        /// "to next division"
        /// </summary>
        public static LocalisableString RatingToNextDivision => new TranslatableString(getKey(@"rating_to_next_division"), @"to next division");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
