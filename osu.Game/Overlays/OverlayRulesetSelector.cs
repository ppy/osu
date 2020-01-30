// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays
{
    public class OverlayRulesetSelector : RulesetSelector
    {
        public OverlayRulesetSelector()
        {
            AutoSizeAxes = Axes.Both;
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new OverlayRulesetTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(25, 0),
        };
    }
}
