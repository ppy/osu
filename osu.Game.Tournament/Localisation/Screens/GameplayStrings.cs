// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class GameplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Screens.Gameplay";

        /// <summary>
        /// "Warmup"
        /// </summary>
        public static LocalisableString Warmup => new TranslatableString(getKey(@"warmup"), @"Warmup");

        /// <summary>
        /// "Show chat"
        /// </summary>
        public static LocalisableString ShowChat => new TranslatableString(getKey(@"show_chat"), @"Show chat");

        /// <summary>
        /// "Chroma width"
        /// </summary>
        public static LocalisableString ChromaWidth => new TranslatableString(getKey(@"chroma_width"), @"Chroma width");

        /// <summary>
        /// "Players per team"
        /// </summary>
        public static LocalisableString PlayersPerTeam => new TranslatableString(getKey(@"players_per_team"), @"Players per team");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
