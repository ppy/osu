// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ButtonSystemStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ButtonSystem";

        /// <summary>
        /// "solo"
        /// </summary>
        public static LocalisableString Solo => new TranslatableString(getKey(@"solo"), @"solo");

        /// <summary>
        /// "multi"
        /// </summary>
        public static LocalisableString Multi => new TranslatableString(getKey(@"multi"), @"multi");

        /// <summary>
        /// "playlists"
        /// </summary>
        public static LocalisableString Playlists => new TranslatableString(getKey(@"playlists"), @"playlists");

        /// <summary>
        /// "play"
        /// </summary>
        public static LocalisableString Play => new TranslatableString(getKey(@"play"), @"play");

        /// <summary>
        /// "edit"
        /// </summary>
        public static LocalisableString Edit => new TranslatableString(getKey(@"edit"), @"edit");

        /// <summary>
        /// "browse"
        /// </summary>
        public static LocalisableString Browse => new TranslatableString(getKey(@"browse"), @"browse");

        /// <summary>
        /// "settings"
        /// </summary>
        public static LocalisableString Settings => new TranslatableString(getKey(@"settings"), @"settings");

        /// <summary>
        /// "back"
        /// </summary>
        public static LocalisableString Back => new TranslatableString(getKey(@"back"), @"back");

        /// <summary>
        /// "exit"
        /// </summary>
        public static LocalisableString Exit => new TranslatableString(getKey(@"exit"), @"exit");

        /// <summary>
        /// "daily challenge"
        /// </summary>
        public static LocalisableString DailyChallenge => new TranslatableString(getKey(@"daily_challenge"), @"daily challenge");

        /// <summary>
        /// "A few important words from your dev team!"
        /// </summary>
        public static LocalisableString MobileDisclaimerHeader => new TranslatableString(getKey(@"mobile_disclaimer_header"), @"A few important words from your dev team!");

        /// <summary>
        /// "While we have released osu! on mobile platforms to maximise the number of people that can enjoy the game, our focus is still on the PC version.
        ///
        /// Your experience will not be perfect, and may even feel subpar compared to games which are made mobile-first.
        ///
        /// Please bear with us as we continue to improve the game for you!"
        /// </summary>
        public static LocalisableString MobileDisclaimerBody => new TranslatableString(getKey(@"mobile_disclaimer_body"),
            @"While we have released osu! on mobile platforms to maximise the number of people that can enjoy the game, our focus is still on the PC version.

Your experience will not be perfect, and may even feel subpar compared to games which are made mobile-first.

Please bear with us as we continue to improve the game for you!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
