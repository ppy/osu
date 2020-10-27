// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchExtraFilterRow : BeatmapSearchMultipleSelectionFilterRow<SearchExtra>
    {
        public BeatmapSearchExtraFilterRow()
            : base("Extra")
        {
        }

        protected override MultipleSelectionFilter CreateMultipleSelectionFilter() => new ExtraFilter();

        private class ExtraFilter : MultipleSelectionFilter
        {
            protected override MultipleSelectionFilterTabItem[] CreateItems() => new MultipleSelectionFilterTabItem[]
            {
                new ExtraFilterTabItem(SearchExtra.Video),
                new ExtraFilterTabItem(SearchExtra.Storyboard)
            };
        }

        private class ExtraFilterTabItem : MultipleSelectionFilterTabItem
        {
            public ExtraFilterTabItem(SearchExtra value)
                : base(value)
            {
            }

            protected override string CreateText(SearchExtra value) => $@"Has {value.ToString()}";
        }
    }
}
