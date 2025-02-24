// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation
{
    public class ScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Custom.Localisation.Tournament.Screen";

        /// <summary>
        /// "Setup"
        /// </summary>
        public static LocalisableString Setup => new TranslatableString(getKey(@"setup"), @"Setup");

        /// <summary>
        /// "Team Editor"
        /// </summary>
        public static LocalisableString TeamEditor => new TranslatableString(getKey(@"team_editor"), @"Team Editor");

        /// <summary>
        /// "Rounds Editor"
        /// </summary>
        public static LocalisableString RoundsEditor => new TranslatableString(getKey(@"rounds_editor"), @"Rounds Editor");

        /// <summary>
        /// "Bracket Editor"
        /// </summary>
        public static LocalisableString BracketEditor => new TranslatableString(getKey(@"bracket_editor"), @"Bracket Editor");

        /// <summary>
        /// "Schedule"
        /// </summary>
        public static LocalisableString Schedule => new TranslatableString(getKey(@"schedule"), @"Schedule");

        /// <summary>
        /// "Bracket"
        /// </summary>
        public static LocalisableString Bracket => new TranslatableString(getKey(@"bracket"), @"Bracket");

        /// <summary>
        /// "Team Intro"
        /// </summary>
        public static LocalisableString TeamIntro => new TranslatableString(getKey(@"team_intro"), @"Team Intro");

        /// <summary>
        /// "Seeding"
        /// </summary>
        public static LocalisableString Seeding => new TranslatableString(getKey(@"seeding"), @"Seeding");

        /// <summary>
        /// "Board"
        /// </summary>
        public static LocalisableString Board => new TranslatableString(getKey(@"board"), @"Board");

        /// <summary>
        /// "Map Pool"
        /// </summary>
        public static LocalisableString MapPool => new TranslatableString(getKey(@"map_pool"), @"Map Pool");

        /// <summary>
        /// "Gameplay"
        /// </summary>
        public static LocalisableString Gameplay => new TranslatableString(getKey(@"gameplay"), @"Gameplay");

        /// <summary>
        /// "Win"
        /// </summary>
        public static LocalisableString Win => new TranslatableString(getKey(@"win"), @"Win");

        /// <summary>
        /// "Drawings"
        /// </summary>
        public static LocalisableString Drawings => new TranslatableString(getKey(@"drawings"), @"Drawings");

        /// <summary>
        /// "Showcase"
        /// </summary>
        public static LocalisableString Showcase => new TranslatableString(getKey(@"showcase"), @"Showcase");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
