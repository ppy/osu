// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class TimeRampModsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.TimeRampMods";

        /// <summary>
        /// "Initial rate"
        /// </summary>
        public static LocalisableString InitialRate => new TranslatableString(getKey(@"initial_rate"), "Initial rate");

        /// <summary>
        /// "The starting speed of the track"
        /// </summary>
        public static LocalisableString InitialRateDescription => new TranslatableString(getKey(@"initial_rate_description"), "The starting speed of the track");

        /// <summary>
        /// "Final rate"
        /// </summary>
        public static LocalisableString FinalRate => new TranslatableString(getKey(@"final_rate"), "Final rate");

        /// <summary>
        /// "The final speed to ramp to"
        /// </summary>
        public static LocalisableString FinalRateDescription => new TranslatableString(getKey(@"final_rate_description"), "The final speed to ramp to");

        /// <summary>
        /// "Adjust pitch"
        /// </summary>
        public static LocalisableString AdjustPitch => new TranslatableString(getKey(@"adjust_pitch"), "Adjust pitch");

        /// <summary>
        /// "Should pitch be adjusted with speed"
        /// </summary>
        public static LocalisableString AdjustPitchDescription => new TranslatableString(getKey(@"adjust_pitch_description"), "Should pitch be adjusted with speed");

        /// <summary>
        /// "Can you keep up?"
        /// </summary>
        public static LocalisableString WindUpDescription => new TranslatableString(getKey(@"wind_up_description"), "Can you keep up?");

        /// <summary>
        /// "Sloooow doooown..."
        /// </summary>
        public static LocalisableString WindDownDescription => new TranslatableString(getKey(@"wind_down_description"), "Sloooow doooown...");

        /// <summary>
        /// "Let track speed adapt to you."
        /// </summary>
        public static LocalisableString AdaptiveSpeedDescription => new TranslatableString(getKey(@"adaptive_speed_description"), "Let track speed adapt to you.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
