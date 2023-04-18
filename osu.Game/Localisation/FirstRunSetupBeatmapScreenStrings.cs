// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class FirstRunSetupBeatmapScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.FirstRunSetupBeatmapScreen";

        /// <summary>
        /// "Obtaining Beatmaps"
        /// </summary>
        public static LocalisableString Header => new TranslatableString(getKey(@"header"), @"Obtaining Beatmaps");

        /// <summary>
        /// "&quot;Beatmaps&quot; are what we call sets of playable levels. osu! doesn&#39;t come with any beatmaps pre-loaded. This step will help you get started on your beatmap collection."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), @"""Beatmaps"" are what we call sets of playable levels. osu! doesn't come with any beatmaps pre-loaded. This step will help you get started on your beatmap collection.");

        /// <summary>
        /// "If you are a new player, we recommend playing through the tutorial to get accustomed to the gameplay."
        /// </summary>
        public static LocalisableString TutorialDescription => new TranslatableString(getKey(@"tutorial_description"), @"If you are a new player, we recommend playing through the tutorial to get accustomed to the gameplay.");

        /// <summary>
        /// "Get the osu! tutorial"
        /// </summary>
        public static LocalisableString TutorialButton => new TranslatableString(getKey(@"tutorial_button"), @"Get the osu! tutorial");

        /// <summary>
        /// "To get you started, we have some recommended beatmaps."
        /// </summary>
        public static LocalisableString BundledDescription => new TranslatableString(getKey(@"bundled_description"), @"To get you started, we have some recommended beatmaps.");

        /// <summary>
        /// "Get recommended beatmaps"
        /// </summary>
        public static LocalisableString BundledButton => new TranslatableString(getKey(@"bundled_button"), @"Get recommended beatmaps");

        /// <summary>
        /// "You can also obtain more beatmaps from the main menu &quot;browse&quot; button at any time."
        /// </summary>
        public static LocalisableString ObtainMoreBeatmaps => new TranslatableString(getKey(@"obtain_more_beatmaps"), @"You can also obtain more beatmaps from the main menu ""browse"" button at any time.");

        /// <summary>
        /// "You currently have {0} beatmap(s) loaded!"
        /// </summary>
        public static LocalisableString CurrentlyLoadedBeatmaps(int beatmaps) => new TranslatableString(getKey(@"currently_loaded_beatmaps"), @"You currently have {0} beatmap(s) loaded!", beatmaps);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
