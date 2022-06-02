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
        /// "osu! is running exclusive fullscreen, guaranteeing low latency!"
        /// </summary>
        public static LocalisableString OsuIsRunningExclusiveFullscreen => new TranslatableString(getKey(@"osu_is_running_exclusive_fullscreen"), @"osu! is running exclusive fullscreen, guaranteeing low latency!");

        /// <summary>
        /// "Unable to run exclusive fullscreen. You&#39;ll still experience some input latency."
        /// </summary>
        public static LocalisableString UnableToRunExclusiveFullscreen => new TranslatableString(getKey(@"unable_to_run_exclusive_fullscreen"), @"Unable to run exclusive fullscreen. You'll still experience some input latency.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}