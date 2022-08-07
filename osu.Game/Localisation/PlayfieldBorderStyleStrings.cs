// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class PlayfieldBorderStyleStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PlayfieldBorderStyle";

        /// <summary>
        /// "Corners"
        /// </summary>
        public static LocalisableString Corners => new TranslatableString(getKey(@"corners"), @"Corners");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
