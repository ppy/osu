// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.HUD
{
    public static class SongProgressStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.HUD.SongProgress";

        /// <summary>
        /// "Difficulty graph type"
        /// </summary>
        public static LocalisableString GraphType => new TranslatableString(getKey(@"graph_type"), "Difficulty graph type");

        /// <summary>
        /// "Type of a graph displaying difficulty throughout the beatmap"
        /// </summary>
        public static LocalisableString GraphTypeDescription => new TranslatableString(getKey(@"graph_type_description"), "Type of a graph displaying difficulty throughout the beatmap");

        /// <summary>
        /// "Show time"
        /// </summary>
        public static LocalisableString ShowTime => new TranslatableString(getKey(@"show_time"), "Show time");

        /// <summary>
        /// "Whether the passed and remaining time should be shown"
        /// </summary>
        public static LocalisableString ShowTimeDescription => new TranslatableString(getKey(@"show_time_description"), "Whether the passed and remaining time should be shown");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
