// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class LayoutSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.LayoutSettings";

        /// <summary>
        /// "Checking for fullscreen capabilities..."
        /// </summary>
        public static LocalisableString CheckingForFullscreenCapabilities => new TranslatableString(getKey(@"checking_for_fullscreen_capabilities"), @"Checking for fullscreen capabilities...");

        /// <summary>
        /// "osu! is running in exclusive fullscreen, guaranteeing low latency!"
        /// </summary>
        public static LocalisableString OsuIsRunningExclusiveFullscreen => new TranslatableString(getKey(@"osu_is_running_exclusive_fullscreen"), @"osu! is running in exclusive fullscreen, guaranteeing low latency!");

        /// <summary>
        /// "Unable to run in exclusive fullscreen. You may experience some input latency."
        /// </summary>
        public static LocalisableString UnableToRunExclusiveFullscreen => new TranslatableString(getKey(@"unable_to_run_exclusive_fullscreen"), @"Unable to run in exclusive fullscreen. You may experience some input latency.");

        /// <summary>
        /// "Using fullscreen on macOS makes interacting with the menu bar and spaces no longer work, and may lead to freezes if a system dialog is presented. Using borderless is recommended."
        /// </summary>
        public static LocalisableString FullscreenMacOSNote => new TranslatableString(getKey(@"fullscreen_macos_note"), @"Using fullscreen on macOS makes interacting with the menu bar and spaces no longer work, and may lead to freezes if a system dialog is presented. Using borderless is recommended.");

        /// <summary>
        /// "Excluding overlays"
        /// </summary>
        public static LocalisableString ScaleEverythingExcludingOverlays => new TranslatableString(getKey(@"scale_everything_excluding_overlays"), @"Excluding overlays");

        /// <summary>
        /// "Everything"
        /// </summary>
        public static LocalisableString ScaleEverything => new TranslatableString(getKey(@"scale_everything"), @"Everything");

        /// <summary>
        /// "Gameplay"
        /// </summary>
        public static LocalisableString ScaleGameplay => new TranslatableString(getKey(@"scale_gameplay"), @"Gameplay");

        /// <summary>
        /// "Off"
        /// </summary>
        public static LocalisableString ScalingOff => new TranslatableString(getKey(@"scaling_off"), @"Off");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
