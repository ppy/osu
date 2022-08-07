// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class BackgroundSourceStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BackgroundSource";

        /// <summary>
        /// "Beatmap (with storyboard / video)"
        /// </summary>
        public static LocalisableString BeatmapWithStoryboard => new TranslatableString(getKey(@"beatmap_with_storyboard"), @"Beatmap (with storyboard / video)");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
