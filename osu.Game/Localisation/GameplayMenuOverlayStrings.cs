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
        /// "You're dead, try again?"
        /// </summary>
        public static LocalisableString FailedDescription => new TranslatableString(getKey(@"failed_description"), @"You're dead, try again?");

        /// <summary>
        /// "You're not going to do what i think you're going to do, are ya?"
        /// </summary>
        public static LocalisableString PausedDescription => new TranslatableString(getKey(@"paused_description"), @"You're not going to do what i think you're going to do, are ya?");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
