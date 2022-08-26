// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchRulesetFilterRow : BeatmapSearchFilterRow<RulesetInfo>
    {
        public BeatmapSearchRulesetFilterRow()
            : base(BeatmapsStrings.ListingSearchFiltersMode)
        {
        }

        protected override Drawable CreateFilter() => new RulesetFilter();

        private class RulesetFilter : BeatmapSearchFilter
        {
            [BackgroundDependencyLoader]
            private void load(RulesetStore rulesets)
            {
                AddTabItem(new RulesetFilterTabItemAny());

                foreach (var r in rulesets.AvailableRulesets)
                    AddItem(r);
            }
        }

        private class RulesetFilterTabItemAny : FilterTabItem<RulesetInfo>
        {
            protected override LocalisableString LabelFor(RulesetInfo info) => BeatmapsStrings.ModeAny;

            public RulesetFilterTabItemAny()
                : base(new RulesetInfo())
            {
            }
        }
    }
}
