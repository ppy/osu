// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Scoring;

namespace osu.Game.Overlays.BeatmapListing
{
    public partial class BeatmapSearchScoreFilterRow : BeatmapSearchMultipleSelectionFilterRow<Grade>
    {
        public BeatmapSearchScoreFilterRow()
            : base(BeatmapsStrings.ListingSearchFiltersRank)
        {
        }

        protected override MultipleSelectionFilter CreateMultipleSelectionFilter() => new RankFilter();

        private partial class RankFilter : MultipleSelectionFilter
        {
            protected override MultipleSelectionFilterTabItem CreateTabItem(Grade value) => new RankItem(value);

            protected override IEnumerable<Grade> GetValues() => base.GetValues().Where(r => r > Grade.F).Reverse();
        }

        private partial class RankItem : MultipleSelectionFilterTabItem
        {
            public RankItem(Grade value)
                : base(value)
            {
            }

            protected override LocalisableString LabelFor(Grade value) => value.GetLocalisableDescription();
        }
    }
}
