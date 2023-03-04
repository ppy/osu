// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class SpectatorListStrings
    {
        private const string prefix = @"osu.Game.resources.Localisation.HUD.SpectatorList";

        /// <summary>
        /// "Spectators"
        /// </summary>
        public static LocalisableString Spectators => new TranslatableString(getKey(@"spectators"), "Spectators");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
