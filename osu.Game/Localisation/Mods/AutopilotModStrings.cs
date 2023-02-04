// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class AutopilotModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.AutopilotMod";

        /// <summary>
        /// "Automatic cursor movement - just follow the rhythm."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "Automatic cursor movement - just follow the rhythm.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
