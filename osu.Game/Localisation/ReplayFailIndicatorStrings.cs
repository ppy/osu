// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ReplayFailIndicatorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ReplayFailIndicator";

        /// <summary>
        /// "Replay failed"
        /// </summary>
        public static LocalisableString ReplayFailed => new TranslatableString(getKey(@"replay_failed"), @"Replay failed");

        /// <summary>
        /// "Go to results"
        /// </summary>
        public static LocalisableString GoToResults => new TranslatableString(getKey(@"go_to_results"), @"Go to results");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}