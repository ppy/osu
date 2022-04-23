using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class JoystickSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.JoystickSettings";

        /// <summary>
        /// "Joystick / Gamepad"
        /// </summary>
        public static LocalisableString JoystickGamepad => new TranslatableString(getKey(@"joystick_gamepad"), @"Joystick / Gamepad");

        /// <summary>
        /// "Deadzone Threshold"
        /// </summary>
        public static LocalisableString DeadzoneThreshold => new TranslatableString(getKey(@"deadzone_threshold"), @"Deadzone Threshold");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}