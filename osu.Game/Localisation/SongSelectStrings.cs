// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SongSelectStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SongSelect";

        /// <summary>
        /// "Local"
        /// </summary>
        public static LocalisableString LocallyModified => new TranslatableString(getKey(@"locally_modified"), @"Local");

        /// <summary>
        /// "Has been locally modified"
        /// </summary>
        public static LocalisableString LocallyModifiedTooltip => new TranslatableString(getKey(@"locally_modified_tooltip"), @"Has been locally modified");

        /// <summary>
        /// "Manage collections"
        /// </summary>
        public static LocalisableString ManageCollections => new TranslatableString(getKey(@"manage_collections"), @"Manage collections");

        /// <summary>
        /// "For all difficulties"
        /// </summary>
        public static LocalisableString ForAllDifficulties => new TranslatableString(getKey(@"for_all_difficulties"), @"For all difficulties");

        /// <summary>
        /// "Delete beatmap"
        /// </summary>
        public static LocalisableString DeleteBeatmap => new TranslatableString(getKey(@"delete_beatmap"), @"Delete beatmap");

        /// <summary>
        /// "For selected difficulty"
        /// </summary>
        public static LocalisableString ForSelectedDifficulty => new TranslatableString(getKey(@"for_selected_difficulty"), @"For selected difficulty");

        /// <summary>
        /// "Mark as played"
        /// </summary>
        public static LocalisableString MarkAsPlayed => new TranslatableString(getKey(@"mark_as_played"), @"Mark as played");

        /// <summary>
        /// "Clear all local scores"
        /// </summary>
        public static LocalisableString ClearAllLocalScores => new TranslatableString(getKey(@"clear_all_local_scores"), @"Clear all local scores");

        /// <summary>
        /// "Edit beatmap"
        /// </summary>
        public static LocalisableString EditBeatmap => new TranslatableString(getKey(@"edit_beatmap"), @"Edit beatmap");

        /// <summary>
        /// "Circle Size"
        /// </summary>
        public static LocalisableString CircleSize => new TranslatableString(getKey(@"circle_size"), @"Circle Size");

        /// <summary>
        /// "Key Count"
        /// </summary>
        public static LocalisableString KeyCount => new TranslatableString(getKey(@"key_count"), @"Key Count");

        /// <summary>
        /// "Approach Rate"
        /// </summary>
        public static LocalisableString ApproachRate => new TranslatableString(getKey(@"approach_rate"), @"Approach Rate");

        /// <summary>
        /// "Accuracy"
        /// </summary>
        public static LocalisableString Accuracy => new TranslatableString(getKey(@"accuracy"), @"Accuracy");

        /// <summary>
        /// "HP Drain"
        /// </summary>
        public static LocalisableString HPDrain => new TranslatableString(getKey(@"hp_drain"), @"HP Drain");

        /// <summary>
        /// "Submitted"
        /// </summary>
        public static LocalisableString Submitted => new TranslatableString(getKey(@"submitted"), @"Submitted");

        /// <summary>
        /// "Ranked"
        /// </summary>
        public static LocalisableString Ranked => new TranslatableString(getKey(@"ranked"), @"Ranked");

        /// <summary>
        /// "{0} stars"
        /// </summary>
        public static LocalisableString Stars(LocalisableString value) => new TranslatableString(getKey(@"stars"), @"{0} stars", value);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
