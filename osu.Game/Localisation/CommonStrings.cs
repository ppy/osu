// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class CommonStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Common";

        /// <summary>
        /// "Cancel"
        /// </summary>
        public static LocalisableString Cancel => new TranslatableString(getKey(@"cancel"), @"Cancel");

        /// <summary>
        /// "Enabled"
        /// </summary>
        public static LocalisableString Enabled => new TranslatableString(getKey(@"enabled"), @"Enabled");

        /// <summary>
        /// "Width"
        /// </summary>
        public static LocalisableString Width => new TranslatableString(getKey(@"width"), @"Width");

        /// <summary>
        /// "Height"
        /// </summary>
        public static LocalisableString Height => new TranslatableString(getKey(@"height"), @"Height");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}