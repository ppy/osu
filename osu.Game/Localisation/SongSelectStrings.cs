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
        /// "Affects the size of hit circles and sliders."
        /// </summary>
        public static LocalisableString OsuCircleSizeDescription => new TranslatableString(getKey(@"osu_circle_size_description"), @"Affects the size of hit circles and sliders.");

        /// <summary>
        /// "Hit circle radius"
        /// </summary>
        public static LocalisableString HitCircleRadius => new TranslatableString(getKey(@"hit_circle_radius"), @"Hit circle radius");

        /// <summary>
        /// "Affects the size of fruits."
        /// </summary>
        public static LocalisableString CatchCircleSizeDescription => new TranslatableString(getKey(@"catch_circle_size_description"), @"Affects the size of fruits.");

        /// <summary>
        /// "Approach Rate"
        /// </summary>
        public static LocalisableString ApproachRate => new TranslatableString(getKey(@"approach_rate"), @"Approach Rate");

        /// <summary>
        /// "Affects how early objects appear on screen relative to their hit time."
        /// </summary>
        public static LocalisableString OsuApproachRateDescription => new TranslatableString(getKey(@"osu_approach_rate_description"), @"Affects how early objects appear on screen relative to their hit time.");

        /// <summary>
        /// "Approach time"
        /// </summary>
        public static LocalisableString ApproachTime => new TranslatableString(getKey(@"approach_time"), @"Approach time");

        /// <summary>
        /// "Affects how early fruits fade in on the screen."
        /// </summary>
        public static LocalisableString CatchApproachRateDescription => new TranslatableString(getKey(@"catch_approach_rate_description"), @"Affects how early fruits fade in on the screen.");

        /// <summary>
        /// "Fade-in time"
        /// </summary>
        public static LocalisableString FadeInTime => new TranslatableString(getKey(@"fade_in_time"), @"Fade-in time");

        /// <summary>
        /// "Accuracy"
        /// </summary>
        public static LocalisableString Accuracy => new TranslatableString(getKey(@"accuracy"), @"Accuracy");

        /// <summary>
        /// "Affects timing requirements for hit circles and spin speed requirements for spinners."
        /// </summary>
        public static LocalisableString OsuAccuracyDescription => new TranslatableString(getKey(@"osu_accuracy_description"), @"Affects timing requirements for hit circles and spin speed requirements for spinners.");

        /// <summary>
        /// "{0} hit window"
        /// </summary>
        public static LocalisableString HitResultWindow(string hitResult) => new TranslatableString(getKey(@"hit_result_window"), @"{0} hit window", hitResult);

        /// <summary>
        /// "RPM required to clear spinners"
        /// </summary>
        public static LocalisableString RpmRequiredToClearSpinners => new TranslatableString(getKey(@"rpm_required_to_clear_spinners"), @"RPM required to clear spinners");

        /// <summary>
        /// "RPM required to get full spinner bonus"
        /// </summary>
        public static LocalisableString RpmRequiredToGetFullSpinnerBonus => new TranslatableString(getKey(@"rpm_required_to_get_full_spinner_bonus"), @"RPM required to get full spinner bonus");

        /// <summary>
        /// "Affects timing requirements for hits and mash rate requirements for swells."
        /// </summary>
        public static LocalisableString TaikoAccuracyDescription => new TranslatableString(getKey(@"taiko_accuracy_description"), @"Affects timing requirements for hits and mash rate requirements for swells.");

        /// <summary>
        /// "Hits per second required to clear swells"
        /// </summary>
        public static LocalisableString HitsPerSecondRequiredToClearSwells => new TranslatableString(getKey(@"hits_per_second_required_to_clear_swells"), @"Hits per second required to clear swells");

        /// <summary>
        /// "Affects timing requirements for notes."
        /// </summary>
        public static LocalisableString ManiaAccuracyDescription => new TranslatableString(getKey(@"mania_accuracy_description"), @"Affects timing requirements for notes.");

        /// <summary>
        /// "HP Drain"
        /// </summary>
        public static LocalisableString HPDrain => new TranslatableString(getKey(@"hp_drain"), @"HP Drain");

        /// <summary>
        /// "Affects the harshness of health drain and the health penalties for missing."
        /// </summary>
        public static LocalisableString HPDrainDescription => new TranslatableString(getKey(@"hp_drain_description"), @"Affects the harshness of health drain and the health penalties for missing.");

        /// <summary>
        /// "Scroll Speed"
        /// </summary>
        public static LocalisableString ScrollSpeed => new TranslatableString(getKey(@"scroll_speed"), @"Scroll Speed");

        /// <summary>
        /// "Multiplier applied to the baseline scroll speed of the playfield when no mods are active."
        /// </summary>
        public static LocalisableString ScrollSpeedDescription => new TranslatableString(getKey(@"scroll_speed_description"), @"Multiplier applied to the baseline scroll speed of the playfield when no mods are active.");

        /// <summary>
        /// "Key Count"
        /// </summary>
        public static LocalisableString KeyCount => new TranslatableString(getKey(@"key_count"), @"Key Count");

        /// <summary>
        /// "Affects the number of key columns on the playfield."
        /// </summary>
        public static LocalisableString KeyCountDescription => new TranslatableString(getKey(@"key_count_description"), @"Affects the number of key columns on the playfield.");

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
        /// "Watch replay"
        /// </summary>
        public static LocalisableString WatchReplay => new TranslatableString(getKey(@"watch_replay"), @"Watch replay");

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

        /// <summary>
        /// "Temporarily showing all beatmaps in"
        /// </summary>
        public static LocalisableString TemporarilyShowingAllBeatmapsIn => new TranslatableString(getKey(@"temporarily_showing_all_beatmaps_in"), @"Temporarily showing all beatmaps in");

        /// <summary>
        /// "mostly {0}"
        /// </summary>
        public static LocalisableString MostlyBPM(int mostCommonBPM) => new TranslatableString(getKey(@"mostly_bpm"), @"mostly {0}", mostCommonBPM);

        /// <summary>
        /// "{0:#,0} match|{0:#,0} matches"
        /// </summary>
        public static LocalisableString MatchesCount(int quantity) => new PluralisableString(new TranslatableString(getKey(@"matches_count"), @"{0:#,0} match|{0:#,0} matches", quantity), quantity, '|');

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
