// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays
{
    public partial class OverlayRulesetSelector : RulesetSelector
    {
        // Since this component is used in online overlays and currently web-side doesn't support non-legacy rulesets, let's disable them for now.
        protected override bool LegacyOnly => true;

        public OverlayRulesetSelector()
        {
            AutoSizeAxes = Axes.Both;
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new OverlayRulesetTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(20, 0),
        };
    }
}
