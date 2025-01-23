// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class TournamentsTabsString
    {
        private const string prefix = @"osu.Game.Resources.Localisation.TournamentsTabs";

        /// <summary>
        /// "info"
        /// </summary>
        public static LocalisableString Info => new TranslatableString(getKey(@"info"), @"info");

        /// <summary>
        /// "players"
        /// </summary>
        public static LocalisableString Players => new TranslatableString(getKey(@"players"), @"players");

        /// <summary>
        /// "qualifiers"
        /// </summary>
        public static LocalisableString Qualifiers => new TranslatableString(getKey(@"qualifiers"), @"qualifiers");

        /// <summary>
        /// "mappools"
        /// </summary>
        public static LocalisableString Mappools => new TranslatableString(getKey(@"mappools"), @"mappools");

        /// <summary>
        /// "bracket"
        /// </summary>
        public static LocalisableString Bracket => new TranslatableString(getKey(@"bracket"), @"bracket");

        /// <summary>
        /// "schedule"
        /// </summary>
        public static LocalisableString Schedule => new TranslatableString(getKey(@"schedule"), @"schedule");

        /// <summary>
        /// "dangerous"
        /// </summary>
        public static LocalisableString Dangerous => new TranslatableString(getKey(@"dangerous"), @"dangerous");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
