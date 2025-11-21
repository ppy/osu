// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SongSelectStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SongSelect";

        /// <summary>
        /// "Mods"
        /// </summary>
        public static LocalisableString Mods => new TranslatableString(getKey(@"mods"), @"Mods");

        /// <summary>
        /// "Random"
        /// </summary>
        public static LocalisableString Random => new TranslatableString(getKey(@"random"), @"Random");

        /// <summary>
        /// "Rewind"
        /// </summary>
        public static LocalisableString Rewind => new TranslatableString(getKey(@"rewind"), @"Rewind");

        /// <summary>
        /// "Options"
        /// </summary>
        public static LocalisableString Options => new TranslatableString(getKey(@"options"), @"Options");

        /// <summary>
        /// "Local"
        /// </summary>
        public static LocalisableString LocallyModified => new TranslatableString(getKey(@"locally_modified"), @"Local");

        /// <summary>
        /// "Has been locally modified"
        /// </summary>
        public static LocalisableString LocallyModifiedTooltip => new TranslatableString(getKey(@"locally_modified_tooltip"), @"Has been locally modified");

        /// <summary>
        /// "Unknown"
        /// </summary>
        public static LocalisableString StatusUnknown => new TranslatableString(getKey(@"status_unknown"), @"Unknown");

        /// <summary>
        /// "Total Plays"
        /// </summary>
        public static LocalisableString TotalPlays => new TranslatableString(getKey(@"total_plays"), @"Total Plays");

        /// <summary>
        /// "Personal Plays"
        /// </summary>
        public static LocalisableString PersonalPlays => new TranslatableString(getKey(@"personal_plays"), @"Personal Plays");

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
        /// "Scroll Speed"
        /// </summary>
        public static LocalisableString ScrollSpeed => new TranslatableString(getKey(@"scroll_speed"), @"Scroll Speed");

        /// <summary>
        /// "Submitted"
        /// </summary>
        public static LocalisableString Submitted => new TranslatableString(getKey(@"submitted"), @"Submitted");

        /// <summary>
        /// "Ranked"
        /// </summary>
        public static LocalisableString Ranked => new TranslatableString(getKey(@"ranked"), @"Ranked");

        /// <summary>
        /// "Details"
        /// </summary>
        public static LocalisableString Details => new TranslatableString(getKey(@"details"), @"Details");

        /// <summary>
        /// "Ranking"
        /// </summary>
        public static LocalisableString Ranking => new TranslatableString(getKey(@"ranking"), @"Ranking");

        /// <summary>
        /// "Use these mods"
        /// </summary>
        public static LocalisableString UseTheseMods => new TranslatableString(getKey(@"use_these_mods"), @"Use these mods");

        /// <summary>
        /// "For all difficulties"
        /// </summary>
        public static LocalisableString ForAllDifficulties => new TranslatableString(getKey(@"for_all_difficulties"), @"For all difficulties");

        /// <summary>
        /// "For selected difficulty"
        /// </summary>
        public static LocalisableString ForSelectedDifficulty => new TranslatableString(getKey(@"for_selected_difficulty"), @"For selected difficulty");

        /// <summary>
        /// "Update beatmap with online changes"
        /// </summary>
        public static LocalisableString UpdateBeatmapTooltip => new TranslatableString(getKey(@"update_beatmap_tooltip"), @"Update beatmap with online changes");

        /// <summary>
        /// "Mark as played"
        /// </summary>
        public static LocalisableString MarkAsPlayed => new TranslatableString(getKey(@"mark_as_played"), @"Mark as played");

        /// <summary>
        /// "Remove from played"
        /// </summary>
        public static LocalisableString RemoveFromPlayed => new TranslatableString(getKey(@"remove_from_played"), @"Remove from played");

        /// <summary>
        /// "Clear all local scores"
        /// </summary>
        public static LocalisableString ClearAllLocalScores => new TranslatableString(getKey(@"clear_all_local_scores"), @"Clear all local scores");

        /// <summary>
        /// "Delete beatmap"
        /// </summary>
        public static LocalisableString DeleteBeatmap => new TranslatableString(getKey(@"delete_beatmap"), @"Delete beatmap");

        /// <summary>
        /// "Restore all hidden"
        /// </summary>
        public static LocalisableString RestoreAllHidden => new TranslatableString(getKey(@"restore_all_hidden"), @"Restore all hidden");

        /// <summary>
        /// "{0} stars"
        /// </summary>
        public static LocalisableString Stars(LocalisableString value) => new TranslatableString(getKey(@"stars"), @"{0} stars", value);

        /// <summary>
        /// "Sort"
        /// </summary>
        public static LocalisableString Sort => new TranslatableString(getKey(@"sort"), @"Sort");

        /// <summary>
        /// "Group"
        /// </summary>
        public static LocalisableString Group => new TranslatableString(getKey(@"group"), @"Group");

        /// <summary>
        /// "None"
        /// </summary>
        public static LocalisableString None => new TranslatableString(getKey(@"none"), @"None");

        /// <summary>
        /// "Title"
        /// </summary>
        public static LocalisableString Title => new TranslatableString(getKey(@"title"), @"Title");

        /// <summary>
        /// "Artist"
        /// </summary>
        public static LocalisableString Artist => new TranslatableString(getKey(@"artist"), @"Artist");

        /// <summary>
        /// "Author"
        /// </summary>
        public static LocalisableString Author => new TranslatableString(getKey(@"author"), @"Author");

        /// <summary>
        /// "BPM"
        /// </summary>
        public static LocalisableString BPM => new TranslatableString(getKey(@"bpm"), @"BPM");

        /// <summary>
        /// "Date Submitted"
        /// </summary>
        public static LocalisableString DateSubmitted => new TranslatableString(getKey(@"date_submitted"), @"Date Submitted");

        /// <summary>
        /// "Date Ranked"
        /// </summary>
        public static LocalisableString DateRanked => new TranslatableString(getKey(@"date_ranked"), @"Date Ranked");

        /// <summary>
        /// "Date Added"
        /// </summary>
        public static LocalisableString DateAdded => new TranslatableString(getKey(@"date_added"), @"Date Added");

        /// <summary>
        /// "Last Played"
        /// </summary>
        public static LocalisableString LastPlayed => new TranslatableString(getKey(@"last_played"), @"Last Played");

        /// <summary>
        /// "Difficulty"
        /// </summary>
        public static LocalisableString Difficulty => new TranslatableString(getKey(@"difficulty"), @"Difficulty");

        /// <summary>
        /// "Length"
        /// </summary>
        public static LocalisableString Length => new TranslatableString(getKey(@"length"), @"Length");

        /// <summary>
        /// "Favourites"
        /// </summary>
        public static LocalisableString Favourites => new TranslatableString(getKey(@"favourites"), @"Favourites");

        /// <summary>
        /// "My Maps"
        /// </summary>
        public static LocalisableString MyMaps => new TranslatableString(getKey(@"my_maps"), @"My Maps");

        /// <summary>
        /// "Collections"
        /// </summary>
        public static LocalisableString Collections => new TranslatableString(getKey(@"collections"), @"Collections");

        /// <summary>
        /// "Rank Achieved"
        /// </summary>
        public static LocalisableString RankAchieved => new TranslatableString(getKey(@"rank_achieved"), @"Rank Achieved");

        /// <summary>
        /// "Ranked Status"
        /// </summary>
        public static LocalisableString RankedStatus => new TranslatableString(getKey(@"ranked_status"), @"Ranked Status");

        /// <summary>
        /// "Source"
        /// </summary>
        public static LocalisableString Source => new TranslatableString(getKey(@"source"), @"Source");

        /// <summary>
        /// "No matching beatmaps"
        /// </summary>
        public static LocalisableString NoMatchingBeatmaps => new TranslatableString(getKey(@"no_matching_beatmaps"), @"No matching beatmaps");

        /// <summary>
        /// "No beatmaps match your filter criteria!"
        /// </summary>
        public static LocalisableString NoMatchingBeatmapsDescription => new TranslatableString(getKey(@"no_matching_beatmaps_description"), @"No beatmaps match your filter criteria!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
