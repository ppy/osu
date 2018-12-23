// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select
{
    public class FilterCriteria
    {
        public GroupMode Group;
        public SortMode Sort;

        public string[] SearchTerms = Array.Empty<string>();

        public RulesetInfo Ruleset;
        public bool AllowConvertedBeatmaps;

        private string searchText;

        public string SearchText
        {
            get { return searchText; }
            set
            {
                searchText = value;
                SearchTerms = searchText.Split(new[] { ',', ' ', '!' }, StringSplitOptions.RemoveEmptyEntries).ToArray();
            }
        }

        public struct OptionalRange : IEquatable<OptionalRange>
        {
            public bool IsTrue(double value)
            {
                if (Min.HasValue && (IsInclusive ? value < Min.Value : value <= Min.Value))
                    return false;
                if (Max.HasValue && (IsInclusive ? value > Max.Value : value >= Max.Value))
                    return false;

                return true;
            }

            public double? Min;
            public double? Max;
            public bool IsInclusive;

            public OptionalRange(double minMax) : this(minMax, minMax) { }
            public OptionalRange(double? min, double? max)
            {
                Min = min;
                Max = max;
                IsInclusive = true;
            }

            public bool Equals(OptionalRange range) => Min == range.Min && Max == range.Max;
        }

        public OptionalRange StarDifficulty;
        public OptionalRange ApproachRate;
        public OptionalRange Length;
        public OptionalRange ObjectCount;

        public int? BeatDivisor;
    }
}
