// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class TabletSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.TabletSettings";

        /// <summary>
        /// "Tablet"
        /// </summary>
        public static LocalisableString Tablet => new TranslatableString(getKey(@"tablet"), @"Tablet");

        /// <summary>
        /// "No tablet detected!"
        /// </summary>
        public static LocalisableString NoTabletDetected => new TranslatableString(getKey(@"no_tablet_detected"), @"No tablet detected!");

        /// <summary>
        /// "If your tablet is not detected, please read [this FAQ]({0}) for troubleshooting steps."
        /// </summary>
        public static LocalisableString NoTabletDetectedDescription(string url) => new TranslatableString(getKey(@"no_tablet_detected_description"), @"If your tablet is not detected, please read [this FAQ]({0}) for troubleshooting steps.", url);

        /// <summary>
        /// "Reset to full area"
        /// </summary>
        public static LocalisableString ResetToFullArea => new TranslatableString(getKey(@"reset_to_full_area"), @"Reset to full area");

        /// <summary>
        /// "Conform to current game aspect ratio"
        /// </summary>
        public static LocalisableString ConformToCurrentGameAspectRatio => new TranslatableString(getKey(@"conform_to_current_game_aspect_ratio"), @"Conform to current game aspect ratio");

        /// <summary>
        /// "X Offset"
        /// </summary>
        public static LocalisableString XOffset => new TranslatableString(getKey(@"x_offset"), @"X Offset");

        /// <summary>
        /// "Y Offset"
        /// </summary>
        public static LocalisableString YOffset => new TranslatableString(getKey(@"y_offset"), @"Y Offset");

        /// <summary>
        /// "Rotation"
        /// </summary>
        public static LocalisableString Rotation => new TranslatableString(getKey(@"rotation"), @"Rotation");

        /// <summary>
        /// "Aspect Ratio"
        /// </summary>
        public static LocalisableString AspectRatio => new TranslatableString(getKey(@"aspect_ratio"), @"Aspect Ratio");

        /// <summary>
        /// "Lock aspect ratio"
        /// </summary>
        public static LocalisableString LockAspectRatio => new TranslatableString(getKey(@"lock_aspect_ratio"), @"Lock aspect ratio");

        /// <summary>
        /// "Tip pressure for click"
        /// </summary>
        public static LocalisableString TipPressureForClick => new TranslatableString(getKey(@"tip_pressure_for_click"), "Tip pressure for click");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
