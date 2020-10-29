// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.BeatmapListing
{
    public class BeatmapSearchRulesetFilterRow : BeatmapSearchFilterRow<RulesetInfo>
    {
        public BeatmapSearchRulesetFilterRow()
            : base(@"模式")
        {
        }

        protected override Drawable CreateFilter() => new RulesetFilter();

        private class RulesetFilter : BeatmapSearchFilter
        {
            [BackgroundDependencyLoader]
            private void load(RulesetStore rulesets)
            {
                AddItem(new RulesetInfo
                {
                    Name = @"所有"
                });

                foreach (var r in rulesets.AvailableRulesets)
                    AddItem(r);
            }
        }
    }
}
