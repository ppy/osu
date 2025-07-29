// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class CollectionsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Collections";

        /// <summary>
        /// "Manage Collections"
        /// </summary>
        public static LocalisableString ManageCollectionsTitle => new TranslatableString(getKey(@"manage_collections_title"), @"Manage Collections");

        /// <summary>
        /// "Collection"
        /// </summary>
        public static LocalisableString Collection => new TranslatableString(getKey(@"collection"), @"Collection");

        /// <summary>
        /// "All beatmaps"
        /// </summary>
        public static LocalisableString AllBeatmaps => new TranslatableString(getKey(@"all_beatmaps"), @"All beatmaps");

        /// <summary>
        /// "Manage collections..."
        /// </summary>
        public static LocalisableString ManageCollections => new TranslatableString(getKey(@"manage_collections"), @"Manage collections...");

        /// <summary>
        /// "Create a new collection"
        /// </summary>
        public static LocalisableString CreateNew => new TranslatableString(getKey(@"create_new"), @"Create a new collection");

        /// <summary>
        /// "Remove selected beatmap"
        /// </summary>
        public static LocalisableString RemoveSelectedBeatmap => new TranslatableString(getKey(@"remove_selected_beatmap"), @"Remove selected beatmap");

        /// <summary>
        /// "Add selected beatmap"
        /// </summary>
        public static LocalisableString AddSelectedBeatmap => new TranslatableString(getKey(@"add_selected_beatmap"), @"Add selected beatmap");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
