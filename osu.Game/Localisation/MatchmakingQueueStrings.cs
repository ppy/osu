// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class MatchmakingQueueStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.MatchmakingQueue";

        /// <summary>
        /// "Searching..."
        /// </summary>
        public static LocalisableString Searching => new TranslatableString(getKey(@"searching"), @"Searching...");

        /// <summary>
        /// "Match found!"
        /// </summary>
        public static LocalisableString MatchFound => new TranslatableString(getKey(@"match_found"), @"Match found!");

        /// <summary>
        /// "Waiting for opponents..."
        /// </summary>
        public static LocalisableString WaitingForOpponents => new TranslatableString(getKey(@"waiting_for_opponents"), @"Waiting for opponents...");

        /// <summary>
        /// "Good luck!"
        /// </summary>
        /// <remarks>
        /// Displayed right before the game transitions from the queue screen to the actual match.
        /// </remarks>
        public static LocalisableString GoodLuck => new TranslatableString(getKey(@"good_luck"), @"Good luck!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
