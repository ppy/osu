// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class FreezeFrameModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.FreezeFrameMod";

        /// <summary>
        /// "Burn the notes into your memory."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "Burn the notes into your memory.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
