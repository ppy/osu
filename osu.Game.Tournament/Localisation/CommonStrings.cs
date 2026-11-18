// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public class CommonStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Common";

        /// <summary>
        /// "Refresh"
        /// </summary>
        public static LocalisableString Refresh => new TranslatableString(getKey(@"refresh"), @"Refresh");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
