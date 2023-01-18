// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class PracticePlayerLoaderStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PracticePlayerLoader";

        /// <summary>
        /// "Practice Mode"
        /// </summary>
        public static LocalisableString PracticeMode => new TranslatableString(getKey(@"practice_mode"), @"Practice Mode");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}