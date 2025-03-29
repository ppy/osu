// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class AimErrorMeterStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.AimErrorMeterStrings";

        /// <summary>
        /// "Hit position size"
        /// </summary>
        public static LocalisableString HitPositionSize => new TranslatableString(getKey(@"hit_position_size"), "Hit position size");

        /// <summary>
        /// "How big of hit position should be."
        /// </summary>
        public static LocalisableString HitPositionSizeDescription => new TranslatableString(getKey("hit_point_size_description"), "How big of hit position should be.");

        /// <summary>
        /// "Hit position style"
        /// </summary>
        public static LocalisableString HitPositionStyle => new TranslatableString(getKey(@"hit_position_style"), "Hit position style");

        /// <summary>
        /// "The style of hit position."
        /// </summary>
        public static LocalisableString HitPositionStyleDescription => new TranslatableString(getKey("hit_position_style_description"), "The style of hit position.");

        /// <summary>
        /// "Average position size"
        /// </summary>
        public static LocalisableString AverageSize => new TranslatableString(getKey(@"average_size"), "Average position size");

        /// <summary>
        /// "How big of average position should be."
        /// </summary>
        public static LocalisableString AverageSizeDescription => new TranslatableString(getKey("average_size_description"), "How big of average position should be.");

        /// <summary>
        /// "Average position style"
        /// </summary>
        public static LocalisableString AverageStyle => new TranslatableString(getKey(@"average_style"), "Average position style");

        /// <summary>
        /// "The style of average position."
        /// </summary>
        public static LocalisableString AverageStyleDescription => new TranslatableString(getKey("average_style_description"), "The style of average position.");

        /// <summary>
        /// "Position mapping"
        /// </summary>
        public static LocalisableString PositionStyle => new TranslatableString(getKey("position_style"), "Position mapping");

        /// <summary>
        /// "Should hit point relative of last object"
        /// </summary>
        public static LocalisableString PositionStyleDescription => new TranslatableString(getKey("position_style_description"), "Should hit point relative of last object");

        /// <summary>
        /// "X"
        /// </summary>
        public static LocalisableString StyleX => new TranslatableString(getKey("style_x"), "X");

        /// <summary>
        /// "+"
        /// </summary>
        public static LocalisableString StylePlus => new TranslatableString(getKey("style_plus"), "+");

        /// <summary>
        /// "Absolute"
        /// </summary>
        public static LocalisableString Absolute => new TranslatableString(getKey("absolute"), "Absolute");

        /// <summary>
        /// "Relative"
        /// </summary>
        public static LocalisableString Relative => new TranslatableString(getKey("relative"), "Relative");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
