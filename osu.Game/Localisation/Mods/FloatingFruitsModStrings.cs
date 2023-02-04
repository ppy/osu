// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class FloatingFruitsModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.FloatingFruitsMod";

        /// <summary>
        /// "The fruits are... floating?"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "The fruits are... floating?");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
