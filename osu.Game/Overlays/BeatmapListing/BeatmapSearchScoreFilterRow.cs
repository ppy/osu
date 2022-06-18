// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchScoreFilterRow : BeatmapSearchMultipleSelectionFilterRow<ScoreRank>
    {
        public BeatmapSearchScoreFilterRow()
            : base(BeatmapsStrings.ListingSearchFiltersRank)
        {
        }

        protected override MultipleSelectionFilter CreateMultipleSelectionFilter() => new RankFilter();

        private class RankFilter : MultipleSelectionFilter
        {
            protected override MultipleSelectionFilterTabItem CreateTabItem(ScoreRank value) => new RankItem(value);

            protected override IEnumerable<ScoreRank> GetValues() => base.GetValues().Reverse();
        }

        private class RankItem : MultipleSelectionFilterTabItem
        {
            public RankItem(ScoreRank value)
                : base(value)
            {
            }

            protected override LocalisableString LabelFor(ScoreRank value) => value.GetLocalisableDescription();
        }
    }
}
