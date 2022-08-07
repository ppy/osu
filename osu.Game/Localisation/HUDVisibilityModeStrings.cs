// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class HUDVisibilityModeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUDVisibilityMode";

        /// <summary>
        /// "Hide during gameplay"
        /// </summary>
        public static LocalisableString HideDuringGameplay => new TranslatableString(getKey(@"hide_during_gameplay"), @"Hide during gameplay");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}