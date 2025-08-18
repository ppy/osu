// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class AimErrorMeterStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.AimErrorMeterStrings";

        /// <summary>
        /// "Hit marker size"
        /// </summary>
        public static LocalisableString HitMarkerSize => new TranslatableString(getKey(@"hit_marker_size"), @"Hit marker size");

        /// <summary>
        /// "Controls the size of the markers displayed after every hit."
        /// </summary>
        public static LocalisableString HitMarkerSizeDescription => new TranslatableString(getKey(@"hit_marker_size_description"), @"Controls the size of the markers displayed after every hit.");

        /// <summary>
        /// "Hit marker style"
        /// </summary>
        public static LocalisableString HitMarkerStyle => new TranslatableString(getKey(@"hit_marker_style"), @"Hit marker style");

        /// <summary>
        /// "The visual style of the hit markers."
        /// </summary>
        public static LocalisableString HitMarkerStyleDescription => new TranslatableString(getKey(@"hit_marker_style_description"), @"The visual style of the hit markers.");

        /// <summary>
        /// "Average position marker size"
        /// </summary>
        public static LocalisableString AverageMarkerSize => new TranslatableString(getKey(@"average_marker_size"), @"Average position marker size");

        /// <summary>
        /// "Controls the size of the marker showing average hit position."
        /// </summary>
        public static LocalisableString AverageMarkerSizeDescription => new TranslatableString(getKey(@"average_marker_size_description"), @"Controls the size of the marker showing average hit position.");

        /// <summary>
        /// "Average position marker style"
        /// </summary>
        public static LocalisableString AverageMarkerStyle => new TranslatableString(getKey(@"average_marker_style"), @"Average position marker style");

        /// <summary>
        /// "The visual style of the average position marker."
        /// </summary>
        public static LocalisableString AverageMarkerStyleDescription => new TranslatableString(getKey(@"average_marker_style_description"), @"The visual style of the average position marker.");

        /// <summary>
        /// "Position display style"
        /// </summary>
        public static LocalisableString PositionDisplayStyle => new TranslatableString(getKey(@"position_style"), @"Position display style");

        /// <summary>
        /// "Controls whether positions displayed on the meter are absolute (as seen on screen) or normalised (relative to the direction of movement from previous object)."
        /// </summary>
        public static LocalisableString PositionDisplayStyleDescription => new TranslatableString(getKey(@"position_style_description"), @"Controls whether positions displayed on the meter are absolute (as seen on screen) or normalised (relative to the direction of movement from previous object).");

        /// <summary>
        /// "Absolute"
        /// </summary>
        public static LocalisableString Absolute => new TranslatableString(getKey(@"absolute"), @"Absolute");

        /// <summary>
        /// "Normalised"
        /// </summary>
        public static LocalisableString Normalised => new TranslatableString(getKey(@"normalised"), @"Normalised");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
