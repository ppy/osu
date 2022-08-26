// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Game.Graphics;
using osu.Game.Resources.Localisation.Web;
using osuTK.Graphics;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchGeneralFilterRow : BeatmapSearchMultipleSelectionFilterRow<SearchGeneral>
    {
        public BeatmapSearchGeneralFilterRow()
            : base(BeatmapsStrings.ListingSearchFiltersGeneral)
        {
        }

        protected override MultipleSelectionFilter CreateMultipleSelectionFilter() => new GeneralFilter();

        private class GeneralFilter : MultipleSelectionFilter
        {
            protected override MultipleSelectionFilterTabItem CreateTabItem(SearchGeneral value)
            {
                if (value == SearchGeneral.FeaturedArtists)
                    return new FeaturedArtistsTabItem();

                return new MultipleSelectionFilterTabItem(value);
            }
        }

        private class FeaturedArtistsTabItem : MultipleSelectionFilterTabItem
        {
            public FeaturedArtistsTabItem()
                : base(SearchGeneral.FeaturedArtists)
            {
            }

            [Resolved]
            private OsuColour colours { get; set; }

            protected override Color4 GetStateColour() => colours.Orange1;
        }
    }
}
