// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class FontStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Font";

        /// <summary>
        /// "Light"
        /// </summary>
        public static LocalisableString Light => new TranslatableString(getKey(@"light"), @"Light");

        /// <summary>
        /// "Regular"
        /// </summary>
        public static LocalisableString Regular => new TranslatableString(getKey(@"regular"), @"Regular");

        /// <summary>
        /// "Medium"
        /// </summary>
        public static LocalisableString Medium => new TranslatableString(getKey(@"medium"), @"Medium");

        /// <summary>
        /// "Semibold"
        /// </summary>
        public static LocalisableString SemiBold => new TranslatableString(getKey(@"semi_bold"), @"Semibold");

        /// <summary>
        /// "Bold"
        /// </summary>
        public static LocalisableString Bold => new TranslatableString(getKey(@"bold"), @"Bold");

        /// <summary>
        /// "Black"
        /// </summary>
        public static LocalisableString Black => new TranslatableString(getKey(@"black"), @"Black");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
