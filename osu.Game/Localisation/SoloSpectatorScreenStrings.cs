// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SoloSpectatorScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SoloSpectatorScreen";

        /// <summary>
        /// "Spectator Mode"
        /// </summary>
        public static LocalisableString SpectatorMode => new TranslatableString(getKey(@"spectator_mode"), @"Spectator Mode");

        /// <summary>
        /// "Start Watching"
        /// </summary>
        public static LocalisableString StartWatching => new TranslatableString(getKey(@"start_watching"), @"Start Watching");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
