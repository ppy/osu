// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;
using System.Linq;

namespace osu.Game.Overlays.Rankings
{
    public class RankingsRulesetSelector : PageTabControl<RulesetInfo>
    {
        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new RankingsTabItem(value);

        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        public RankingsRulesetSelector()
        {
            AutoSizeAxes = Axes.X;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, RulesetStore rulesets)
        {
            foreach (var r in rulesets.AvailableRulesets)
                AddItem(r);

            AccentColour = colours.Lime;

            SelectTab(TabContainer.FirstOrDefault());
        }

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.X,
            RelativeSizeAxes = Axes.Y,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(20, 0),
        };

        private class RankingsTabItem : PageTabItem
        {
            public RankingsTabItem(RulesetInfo value)
                : base(value)
            {
            }

            protected override string CreateText() => $"{Value.Name}";
        }
    }
}
