// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ModCustomizationSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ModCustomizationSettings";

        /// <summary>
        /// "From Previous Hit Circle"
        /// </summary>
        public static LocalisableString FromPrevious => new TranslatableString(getKey(@"from_previous"), @"From Previous Hit Circle");

        /// <summary>
        /// "Towards Previous Hit Circle"
        /// </summary>
        public static LocalisableString TowardsPrevious => new TranslatableString(getKey(@"towards_previous"), @"Towards Previous Hit Circle");

        /// <summary>
        /// "Random"
        /// </summary>
        public static LocalisableString RandomDirection => new TranslatableString(getKey(@"random_direction"), @"Random");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
