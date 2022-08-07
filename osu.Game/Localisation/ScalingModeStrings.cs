// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ScalingModeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ScalingMode";

        /// <summary>
        /// "Excluding overlays"
        /// </summary>
        public static LocalisableString ExcludingOverlays => new TranslatableString(getKey(@"excluding_overlays"), @"Excluding overlays");

        /// <summary>
        /// "Everything"
        /// </summary>
        public static LocalisableString Everything => new TranslatableString(getKey(@"everything"), @"Everything");

        /// <summary>
        /// "Gameplay"
        /// </summary>
        public static LocalisableString Gameplay => new TranslatableString(getKey(@"gameplay"), @"Gameplay");

        /// <summary>
        /// "Off"
        /// </summary>
        public static LocalisableString Off => new TranslatableString(getKey(@"scaling_mode.off"), @"Off");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
