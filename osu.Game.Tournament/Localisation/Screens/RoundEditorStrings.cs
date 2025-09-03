// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class RoundEditorStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Screens.RoundEditor";

        /// <summary>
        /// "Round Information"
        /// </summary>
        public static LocalisableString RoundInfoHeader => new TranslatableString(getKey(@"round_info_header"), @"Round Information");

        /// <summary>
        /// "Name"
        /// </summary>
        public static LocalisableString RoundName => new TranslatableString(getKey(@"round_name"), @"Name");

        /// <summary>
        /// "Description"
        /// </summary>
        public static LocalisableString RoundDescription => new TranslatableString(getKey(@"round_description"), @"Description");

        /// <summary>
        /// "Start Time"
        /// </summary>
        public static LocalisableString StartTime => new TranslatableString(getKey(@"start_time"), @"Start Time");

        /// <summary>
        /// "# of Bans"
        /// </summary>
        public static LocalisableString NumOfBans => new TranslatableString(getKey(@"num_of_bans"), @"# of Bans");

        /// <summary>
        /// "Best of"
        /// </summary>
        public static LocalisableString BestOf => new TranslatableString(getKey(@"best_of"), @"Best of");

        /// <summary>
        /// "Delete Round"
        /// </summary>
        public static LocalisableString DeleteRound => new TranslatableString(getKey(@"delete_round"), @"Delete Round");

        /// <summary>
        /// "Beatmap ID"
        /// </summary>
        public static LocalisableString BeatmapID => new TranslatableString(getKey(@"beatmap_id"), @"Beatmap ID");

        /// <summary>
        /// "Mods"
        /// </summary>
        public static LocalisableString Mods => new TranslatableString(getKey(@"mods"), @"Mods");

        /// <summary>
        /// "Add beatmap"
        /// </summary>
        public static LocalisableString AddBeatmap => new TranslatableString(getKey(@"add_beatmap"), @"Add beatmap");

        /// <summary>
        /// "Delete beatmap"
        /// </summary>
        public static LocalisableString DeleteBeatmap => new TranslatableString(getKey(@"delete_beatmap"), @"Delete beatmap");

        /// <summary>
        /// "Delete result"
        /// </summary>
        public static LocalisableString DeleteResult => new TranslatableString(getKey(@"delete_result"), @"Delete result");

        /// <summary>
        /// "Seed"
        /// </summary>
        public static LocalisableString Seed => new TranslatableString(getKey(@"seed"), @"Seed");

        /// <summary>
        /// "Score"
        /// </summary>
        public static LocalisableString Score => new TranslatableString(getKey(@"score"), @"Score");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

