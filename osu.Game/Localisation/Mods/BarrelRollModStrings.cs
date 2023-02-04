// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class BarrelRollModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.BarrelRollMod";

        /// <summary>
        /// "The whole playfield is on a wheel!"
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), "The whole playfield is on a wheel!");

        /// <summary>
        /// "Roll speed"
        /// </summary>
        public static LocalisableString SpinSpeed => new TranslatableString(getKey(@"spin_speed"), "Roll speed");

        /// <summary>
        /// "Rotations per minute"
        /// </summary>
        public static LocalisableString SpinSpeedDescription => new TranslatableString(getKey(@"spin_speed_description"), "Rotations per minute");

        /// <summary>
        /// "Direction"
        /// </summary>
        public static LocalisableString Direction => new TranslatableString(getKey(@"direction"), "Direction");

        /// <summary>
        /// "The direction of rotation"
        /// </summary>
        public static LocalisableString DirectionDescription => new TranslatableString(getKey(@"direction_description"), "The direction of rotation");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
