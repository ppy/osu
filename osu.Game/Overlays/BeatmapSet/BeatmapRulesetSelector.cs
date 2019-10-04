// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osuTK;
using System.Linq;

namespace osu.Game.Overlays.BeatmapSet
{
    public class BeatmapRulesetSelector : RulesetSelector
    {
        private BeatmapSetInfo beatmapSet;

        public BeatmapSetInfo BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet)
                    return;

                beatmapSet = value;

                foreach (var tab in TabContainer.TabItems.OfType<BeatmapRulesetTabItem>())
                    tab.SetBeatmaps(beatmapSet?.Beatmaps.FindAll(b => b.Ruleset.Equals(tab.Value)));

                var firstRuleset = beatmapSet?.Beatmaps.OrderBy(b => b.Ruleset.ID).FirstOrDefault()?.Ruleset;
                SelectTab(TabContainer.TabItems.FirstOrDefault(t => t.Value.Equals(firstRuleset)));
            }
        }

        public BeatmapRulesetSelector()
        {
            AutoSizeAxes = Axes.Both;
            TabContainer.Spacing = new Vector2(10, 0);
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new BeatmapRulesetTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            Direction = FillDirection.Horizontal,
            AutoSizeAxes = Axes.Both,
        };
    }
}
