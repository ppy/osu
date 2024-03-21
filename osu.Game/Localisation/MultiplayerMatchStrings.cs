// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class MultiplayerMatchStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.MultiplayerMatchStrings";

        /// <summary>
        /// "Stop countdown"
        /// </summary>
        public static LocalisableString StopCountdown => new TranslatableString(getKey(@"stop_countdown"), @"Stop countdown");

        /// <summary>
        /// "Countdown settings"
        /// </summary>
        public static LocalisableString CountdownSettings => new TranslatableString(getKey(@"countdown_settings"), @"Countdown settings");

        /// <summary>
        /// "Start match in {0}"
        /// </summary>
        public static LocalisableString StartMatchWithCountdown(string humanReadableTime) => new TranslatableString(getKey(@"start_match_width_countdown"), @"Start match in {0}", humanReadableTime);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
