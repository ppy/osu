// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
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
        private readonly Bindable<BeatmapSetInfo> beatmapSet = new Bindable<BeatmapSetInfo>();

        public BeatmapSetInfo BeatmapSet
        {
            get => beatmapSet.Value;
            set
            {
                // propagate value to tab items first to enable only available rulesets.
                beatmapSet.Value = value;

                SelectTab(TabContainer.TabItems.FirstOrDefault(t => t.Enabled.Value));
            }
        }

        public BeatmapRulesetSelector()
        {
            AutoSizeAxes = Axes.Both;
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new BeatmapRulesetTabItem(value)
        {
            BeatmapSet = { BindTarget = beatmapSet }
        };

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(10, 0),
        };
    }
}
