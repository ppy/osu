// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Extensions;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchRankFilterRow : BeatmapSearchMultipleSelectionFilterRow<SearchRank>
    {
        public BeatmapSearchRankFilterRow()
            : base("Rank Achieved")
        {
        }

        protected override MultipleSelectionFilter CreateMultipleSelectionFilter() => new RankFilter();

        private class RankFilter : MultipleSelectionFilter
        {
            protected override MultipleSelectionFilterTabItem[] CreateItems()
                => ((SearchRank[])Enum.GetValues(typeof(SearchRank))).Select(v => new RankFilterTabItem(v)).ToArray<MultipleSelectionFilterTabItem>();
        }

        private class RankFilterTabItem : MultipleSelectionFilterTabItem
        {
            public RankFilterTabItem(SearchRank value)
                : base(value)
            {
            }

            protected override string CreateText(SearchRank value) => $@"{value.GetDescription() ?? value.ToString()}";
        }
    }
}
