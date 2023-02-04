// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class FunModsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.FunMods";

        /// <summary>
        /// "The fruits are... floating?"
        /// </summary>
        public static LocalisableString FloatingFruitsDescription => new TranslatableString(getKey(@"floating_fruits_description"), "The fruits are... floating?");

        /// <summary>
        /// "Burn the notes into your memory."
        /// </summary>
        public static LocalisableString FreezeFrameDescription => new TranslatableString(getKey(@"freeze_frame_description"), "Burn the notes into your memory.");

        private static string getKey(string key) => $"{prefix}:{key}";

        /// <summary>
        /// "Circles spin in. No approach circles."
        /// </summary>
        public static LocalisableString SpinInDescription => new TranslatableString(getKey(@"spin_in_description"), "Circles spin in. No approach circles.");
    }
}
