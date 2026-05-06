// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class MatchmakingStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.MatchmakingStrings";

        /// <summary>
        /// "History"
        /// </summary>
        public static LocalisableString History => new TranslatableString(getKey(@"history"), @"History");

        /// <summary>
        /// "How you played"
        /// </summary>
        public static LocalisableString HowYouPlayed => new TranslatableString(getKey(@"how_you_played"), @"How you played");

        /// <summary>
        /// "Room Awards"
        /// </summary>
        public static LocalisableString RoomAwards => new TranslatableString(getKey(@"room_awards"), @"Room Awards");

        /// <summary>
        /// "Your final placement"
        /// </summary>
        public static LocalisableString FinalPlacement => new TranslatableString(getKey(@"final_placement"), @"Your final placement");

        /// <summary>
        /// "Waiting for other users"
        /// </summary>
        public static LocalisableString WaitingForClientsJoin => new TranslatableString(getKey(@"waiting_for_clients_join"), @"Waiting for other users");

        /// <summary>
        /// "Players are joining the match..."
        /// </summary>
        public static LocalisableString WaitingForClientsJoinStatus => new TranslatableString(getKey(@"waiting_for_clients_join_status"), @"Players are joining the match...");

        /// <summary>
        /// "Next Round"
        /// </summary>
        public static LocalisableString RoundWarmupTime => new TranslatableString(getKey(@"warmup_time"), @"Next Round");

        /// <summary>
        /// "Beatmap Selection"
        /// </summary>
        public static LocalisableString BeatmapSelect => new TranslatableString(getKey(@"beatmap_select"), @"Beatmap Selection");

        /// <summary>
        /// "Players are downloading the beatmap..."
        /// </summary>
        public static LocalisableString WaitingForClientsBeatmapDownloadStatus => new TranslatableString(getKey(@"waiting_for_clients_beatmap_download_status"), @"Players are downloading the beatmap...");

        /// <summary>
        /// "Get Ready"
        /// </summary>
        public static LocalisableString GameplayWarmupTime => new TranslatableString(getKey(@"gameplay_warmup_time"), @"Get Ready");

        /// <summary>
        /// "Game is in progress..."
        /// </summary>
        public static LocalisableString GameplayStatus => new TranslatableString(getKey(@"gameplay_status"), @"Game is in progress...");

        /// <summary>
        /// "Results"
        /// </summary>
        public static LocalisableString ResultsDisplaying => new TranslatableString(getKey(@"results_displaying"), @"Results");

        /// <summary>
        /// "Match End"
        /// </summary>
        public static LocalisableString Ended => new TranslatableString(getKey(@"ended"), @"Match End");

        /// <summary>
        /// "Thanks for playing! The match will close shortly."
        /// </summary>
        public static LocalisableString EndedStatus => new TranslatableString(getKey(@"ended_status"), @"Thanks for playing! The match will close shortly.");


        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
