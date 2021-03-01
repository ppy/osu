// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Rulesets.Filter
{
    public partial class FilterCriteria
    {
        public GroupMode Group;
        public SortMode Sort;

        public BeatmapSetInfo SelectedBeatmapSet;

        public OptionalRange<double> StarDifficulty;
        public OptionalRange<float> ApproachRate;
        public OptionalRange<float> DrainRate;
        public OptionalRange<float> CircleSize;
        public OptionalRange<double> Length;
        public OptionalRange<double> BPM;
        public OptionalRange<int> BeatDivisor;
        public OptionalRange<BeatmapSetOnlineStatus> OnlineStatus;
        public OptionalTextFilter Creator;
        public OptionalTextFilter Artist;

        public OptionalRange<double> UserStarDifficulty = new OptionalRange<double>
        {
            IsLowerInclusive = true,
            IsUpperInclusive = true
        };

        public string[] SearchTerms = Array.Empty<string>();

        [CanBeNull]
        public RulesetInfo Ruleset;

        public bool AllowConvertedBeatmaps;

        private string searchText;

        /// <summary>
        /// <see cref="SearchText"/> as a number (if it can be parsed as one).
        /// </summary>
        public int? SearchNumber { get; private set; }

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                SearchTerms = searchText.Split(new[] { ',', ' ', '!' }, StringSplitOptions.RemoveEmptyEntries).ToArray();

                SearchNumber = null;

                if (SearchTerms.Length == 1 && int.TryParse(SearchTerms[0], out int parsed))
                    SearchNumber = parsed;
            }
        }

        /// <summary>
        /// The collection to filter beatmaps from.
        /// </summary>
        [CanBeNull]
        public BeatmapCollection Collection;

        public FilterCriteria(RulesetInfo ruleset, FilterCreationParameters parameters)
        {
            Group = parameters.GroupMode;
            Sort = parameters.SortMode;
            AllowConvertedBeatmaps = parameters.AllowConvertedBeatmaps;
            Ruleset = ruleset;
            Collection = parameters.Collection;
            UserStarDifficulty = parameters.UserStarDifficulty;

            applyQueries(parameters.Query);
        }

        internal FilterCriteria()
        {
        }

        /// <summary>
        /// Checks whether the supplied <paramref name="beatmap"/> satisfies the criteria represented
        /// by this instance.
        /// </summary>
        /// <param name="beatmap">The beatmap to test the criteria against.</param>
        /// <returns><c>true</c> if the beatmap matches the current filtering criteria, <c>false</c> otherwise.</returns>
        public bool Matches(BeatmapInfo beatmap)
        {
            bool match =
                Ruleset == null ||
                beatmap.RulesetID == Ruleset.ID ||
                (beatmap.RulesetID == 0 && Ruleset.ID > 0 && AllowConvertedBeatmaps);

            if (beatmap.BeatmapSet?.Equals(SelectedBeatmapSet) == true)
            {
                // only check ruleset equality or convertability for selected beatmap
                return match;
            }

            match &= !StarDifficulty.HasFilter || StarDifficulty.IsInRange(beatmap.StarDifficulty);
            match &= !ApproachRate.HasFilter || ApproachRate.IsInRange(beatmap.BaseDifficulty.ApproachRate);
            match &= !DrainRate.HasFilter || DrainRate.IsInRange(beatmap.BaseDifficulty.DrainRate);
            match &= !CircleSize.HasFilter || CircleSize.IsInRange(beatmap.BaseDifficulty.CircleSize);
            match &= !Length.HasFilter || Length.IsInRange(beatmap.Length);
            match &= !BPM.HasFilter || BPM.IsInRange(beatmap.BPM);

            match &= !BeatDivisor.HasFilter || BeatDivisor.IsInRange(beatmap.BeatDivisor);
            match &= !OnlineStatus.HasFilter || OnlineStatus.IsInRange(beatmap.Status);

            match &= !Creator.HasFilter || Creator.Matches(beatmap.Metadata.AuthorString);
            match &= !Artist.HasFilter || Artist.Matches(beatmap.Metadata.Artist) ||
                     Artist.Matches(beatmap.Metadata.ArtistUnicode);

            match &= !UserStarDifficulty.HasFilter || UserStarDifficulty.IsInRange(beatmap.StarDifficulty);

            if (match)
            {
                var terms = beatmap.SearchableTerms;

                foreach (var criteriaTerm in SearchTerms)
                    match &= terms.Any(term => term.Contains(criteriaTerm, StringComparison.InvariantCultureIgnoreCase));

                // if a match wasn't found via text matching of terms, do a second catch-all check matching against online IDs.
                // this should be done after text matching so we can prioritise matching numbers in metadata.
                if (!match && SearchNumber.HasValue)
                {
                    match = (beatmap.OnlineBeatmapID == SearchNumber.Value) ||
                            (beatmap.BeatmapSet?.OnlineBeatmapSetID == SearchNumber.Value);
                }
            }

            if (match)
                match &= Collection?.Beatmaps.Contains(beatmap) ?? true;

            if (match)
                match &= MatchesCustomCriteria(beatmap);

            return match;
        }

        /// <summary>
        /// Checks whether the supplied <paramref name="beatmap"/> satisfies ruleset-specific custom criteria,
        /// in addition to the ones mandated by song select.
        /// </summary>
        /// <param name="beatmap">The beatmap to test the criteria against.</param>
        /// <returns>
        /// <c>true</c> if the beatmap matches the ruleset-specific custom filtering criteria,
        /// <c>false</c> otherwise.
        /// </returns>
        protected virtual bool MatchesCustomCriteria(BeatmapInfo beatmap) => true;

        public struct OptionalRange<T> : IEquatable<OptionalRange<T>>
            where T : struct
        {
            public bool HasFilter => Max != null || Min != null;

            public bool IsInRange(T value)
            {
                if (Min != null)
                {
                    int comparison = Comparer<T>.Default.Compare(value, Min.Value);

                    if (comparison < 0)
                        return false;

                    if (comparison == 0 && !IsLowerInclusive)
                        return false;
                }

                if (Max != null)
                {
                    int comparison = Comparer<T>.Default.Compare(value, Max.Value);

                    if (comparison > 0)
                        return false;

                    if (comparison == 0 && !IsUpperInclusive)
                        return false;
                }

                return true;
            }

            public T? Min;
            public T? Max;
            public bool IsLowerInclusive;
            public bool IsUpperInclusive;

            public bool Equals(OptionalRange<T> other)
                => EqualityComparer<T?>.Default.Equals(Min, other.Min)
                   && EqualityComparer<T?>.Default.Equals(Max, other.Max)
                   && IsLowerInclusive.Equals(other.IsLowerInclusive)
                   && IsUpperInclusive.Equals(other.IsUpperInclusive);
        }

        public struct OptionalTextFilter : IEquatable<OptionalTextFilter>
        {
            public bool HasFilter => !string.IsNullOrEmpty(SearchTerm);

            public bool Matches(string value)
            {
                if (!HasFilter)
                    return true;

                // search term is guaranteed to be non-empty, so if the string we're comparing is empty, it's not matching
                if (string.IsNullOrEmpty(value))
                    return false;

                return value.Contains(SearchTerm, StringComparison.InvariantCultureIgnoreCase);
            }

            public string SearchTerm;

            public bool Equals(OptionalTextFilter other) => SearchTerm == other.SearchTerm;
        }
    }
}
