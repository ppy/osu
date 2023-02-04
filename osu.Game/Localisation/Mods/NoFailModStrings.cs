// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class NoFailModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.NoFail";

        /// <summary>
        /// "You can't fail, no matter what."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "You can't fail, no matter what.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
