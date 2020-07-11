// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    public class FilterCriteria
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

        public RulesetInfo Ruleset;
        public bool AllowConvertedBeatmaps;

        private string searchText;

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                SearchTerms = searchText.Split(new[] { ',', ' ', '!' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            }
        }

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

                return value.IndexOf(SearchTerm, StringComparison.InvariantCultureIgnoreCase) >= 0;
            }

            public string SearchTerm;

            public bool Equals(OptionalTextFilter other) => SearchTerm == other.SearchTerm;
        }
    }
}
