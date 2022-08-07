// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class OsuConfineMouseModeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.OsuConfineMouseMode";

        /// <summary>
        /// "During Gameplay"
        /// </summary>
        public static LocalisableString DuringGameplay => new TranslatableString(getKey(@"during_gameplay"), @"During Gameplay");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
