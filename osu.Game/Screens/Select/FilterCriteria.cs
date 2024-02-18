﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Filter;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    public class FilterCriteria
    {
        public GroupMode Group;
        public SortMode Sort;

        /// <summary>
        /// Whether the display of beatmap sets should be split apart per-difficulty for the current criteria.
        /// </summary>
        public bool SplitOutDifficulties => Sort == SortMode.Difficulty;

        public BeatmapSetInfo? SelectedBeatmapSet;

        public OptionalRange<double> StarDifficulty;
        public OptionalRange<float> ApproachRate;
        public OptionalRange<float> DrainRate;
        public OptionalRange<float> CircleSize;
        public OptionalRange<float> OverallDifficulty;
        public OptionalRange<double> Length;
        public OptionalRange<double> BPM;
        public OptionalRange<int> BeatDivisor;
        public OptionalRange<BeatmapOnlineStatus> OnlineStatus;
        public OptionalRange<DateTimeOffset> LastPlayed;
        public OptionalTextFilter Creator;
        public OptionalTextFilter Artist;
        public OptionalTextFilter Title;
        public OptionalTextFilter DifficultyName;

        public OptionalRange<double> UserStarDifficulty = new OptionalRange<double>
        {
            IsLowerInclusive = true,
            IsUpperInclusive = true
        };

        public OptionalTextFilter[] SearchTerms = Array.Empty<OptionalTextFilter>();

        public RulesetInfo? Ruleset;
        public bool AllowConvertedBeatmaps;

        private string searchText = string.Empty;

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

                List<OptionalTextFilter> terms = new List<OptionalTextFilter>();

                string remainingText = value;

                // Match either an open difficulty tag to the end of string,
                // or match a closed one with a whitespace after it.
                //
                // To keep things simple, the closing ']' may be included in the match group,
                // and is trimmed post-match.
                foreach (Match quotedSegment in Regex.Matches(value, "(^|\\s)\\[(.*)(\\]\\s|$)"))
                {
                    DifficultyName = new OptionalTextFilter
                    {
                        SearchTerm = quotedSegment.Groups[2].Value.Trim(']')
                    };

                    remainingText = remainingText.Replace(quotedSegment.Value, string.Empty);
                }

                // First handle quoted segments to ensure we keep inline spaces in exact matches.
                foreach (Match quotedSegment in Regex.Matches(value, "(\"[^\"]+\"[!]?)"))
                {
                    terms.Add(new OptionalTextFilter { SearchTerm = quotedSegment.Value });
                    remainingText = remainingText.Replace(quotedSegment.Value, string.Empty);
                }

                // Then handle the rest splitting on any spaces.
                terms.AddRange(remainingText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(s => new OptionalTextFilter
                {
                    SearchTerm = s
                }));

                SearchTerms = terms.ToArray();

                SearchNumber = null;

                if (SearchTerms.Length == 1 && int.TryParse(SearchTerms[0].SearchTerm, out int parsed))
                    SearchNumber = parsed;
            }
        }

        /// <summary>
        /// Hashes from the <see cref="BeatmapCollection"/> to filter to.
        /// </summary>
        public IEnumerable<string>? CollectionBeatmapMD5Hashes { get; set; }

        public IRulesetFilterCriteria? RulesetCriteria { get; set; }

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

            public MatchMode MatchMode { get; private set; }

            public bool Matches(string value)
            {
                if (!HasFilter)
                    return true;

                // search term is guaranteed to be non-empty, so if the string we're comparing is empty, it's not matching
                if (string.IsNullOrEmpty(value))
                    return false;

                switch (MatchMode)
                {
                    default:
                    case MatchMode.Substring:
                        // Note that we are using ordinal here to avoid performance issues caused by globalisation concerns.
                        // See https://github.com/ppy/osu/issues/11571 / https://github.com/dotnet/docs/issues/18423.
                        return value.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase);

                    case MatchMode.IsolatedPhrase:
                        return Regex.IsMatch(value, $@"(^|\s){Regex.Escape(searchTerm)}($|\s)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

                    case MatchMode.FullPhrase:
                        return CultureInfo.InvariantCulture.CompareInfo.Compare(value, searchTerm, CompareOptions.OrdinalIgnoreCase) == 0;
                }
            }

            private string searchTerm;

            public string SearchTerm
            {
                get => searchTerm;
                set
                {
                    searchTerm = value;

                    if (searchTerm.StartsWith('\"'))
                    {
                        // length check ensures that the quote character in the `StartsWith()` check above and the `EndsWith()` check below is not the same character.
                        if (searchTerm.EndsWith("\"!", StringComparison.Ordinal) && searchTerm.Length >= 3)
                        {
                            searchTerm = searchTerm.TrimEnd('!').Trim('\"');
                            MatchMode = MatchMode.FullPhrase;
                        }
                        else
                        {
                            searchTerm = searchTerm.Trim('\"');
                            MatchMode = MatchMode.IsolatedPhrase;
                        }
                    }
                    else
                        MatchMode = MatchMode.Substring;
                }
            }

            public bool Equals(OptionalTextFilter other) => SearchTerm == other.SearchTerm;
        }

        /// <summary>
        /// Given a new filter criteria, decide whether a full sort needs to be performed.
        /// </summary>
        /// <param name="newCriteria"></param>
        /// <returns></returns>
        public bool RequiresSorting(FilterCriteria newCriteria)
        {
            if (Sort != newCriteria.Sort)
                return true;

            switch (Sort)
            {
                // Some sorts are stable across all other changes.
                // Running these sorts will sort all items, including currently hidden items.
                case SortMode.Artist:
                case SortMode.Author:
                case SortMode.DateSubmitted:
                case SortMode.DateAdded:
                case SortMode.DateRanked:
                case SortMode.Source:
                case SortMode.Title:
                    return false;

                // Some sorts use aggregate max comparisons, which will change based on filtered items.
                // These sorts generally ignore items hidden by filtered state, so we must force a sort under all circumstances here.
                //
                // This makes things very slow when typing a text search, and we probably want to consider a way to optimise things going forward.
                case SortMode.LastPlayed:
                case SortMode.BPM:
                case SortMode.Length:
                case SortMode.Difficulty:
                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(Sort), Sort, "Unknown sort mode");
            }
        }

        public enum MatchMode
        {
            /// <summary>
            /// Match using a simple "contains" substring match.
            /// </summary>
            Substring,

            /// <summary>
            /// Match for the search phrase being isolated by spaces, or at the start or end of the text.
            /// </summary>
            IsolatedPhrase,

            /// <summary>
            /// Match for the search phrase matching the full text in completion.
            /// </summary>
            FullPhrase,
        }
    }
}
