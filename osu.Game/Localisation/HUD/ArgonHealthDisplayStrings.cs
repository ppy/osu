// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class ArgonHealthDisplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.ArgonHealthDisplay";

        /// <summary>
        /// "Bar height"
        /// </summary>
        public static LocalisableString BarHeight => new TranslatableString(getKey(@"bar_height"), @"Bar height");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
