// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class GameplayMenuOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.GameplayMenuOverlay";

        /// <summary>
        /// "Continue"
        /// </summary>
        public static LocalisableString Continue => new TranslatableString(getKey(@"continue"), @"Continue");

        /// <summary>
        /// "Retry"
        /// </summary>
        public static LocalisableString Retry => new TranslatableString(getKey(@"retry"), @"Retry");

        /// <summary>
        /// "Quit"
        /// </summary>
        public static LocalisableString Quit => new TranslatableString(getKey(@"quit"), @"Quit");

        /// <summary>
        /// "failed"
        /// </summary>
        public static LocalisableString FailedHeader => new TranslatableString(getKey(@"failed_header"), @"failed");

        /// <summary>
        /// "paused"
        /// </summary>
        public static LocalisableString PausedHeader => new TranslatableString(getKey(@"paused_header"), @"paused");

        /// <summary>
        /// "Retry count: "
        /// </summary>
        public static LocalisableString RetryCount => new TranslatableString(getKey(@"retry_count"), @"Retry count: ");

        /// <summary>
        /// "Song progress: "
        /// </summary>
        public static LocalisableString SongProgress => new TranslatableString(getKey(@"song_progress"), @"Song progress: ");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}