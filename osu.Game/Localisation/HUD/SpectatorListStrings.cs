// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class SpectatorListStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SpectatorList";

        /// <summary>
        /// "Spectators ({0})"
        /// </summary>
        public static LocalisableString SpectatorCount(int arg0) => new TranslatableString(getKey(@"spectator_count"), @"Spectators ({0})", arg0);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
