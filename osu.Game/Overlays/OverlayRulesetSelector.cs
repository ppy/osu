// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;

namespace osu.Game.Overlays
{
    public partial class OverlayRulesetSelector : RulesetSelector
    {
        // Since this component is used in online overlays and currently web-side doesn't support non-legacy rulesets, let's disable them for now.
        protected override bool LegacyOnly => true;

        protected Drawable BottomBar { get; private set; }

        protected int BottomBarHeight = 5;

        public OverlayRulesetSelector()
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;

            AddRangeInternal(new[]
            {
                BottomBar = new Container
                {
                    Height = BottomBarHeight,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Y = -1,
                    Child = new Circle
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.Both
                    }
                }
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindValueChanged(_ => UpdateBottomBarPosition(), true);
        }

        protected void UpdateBottomBarPosition()
        {
            if (SelectedTab != null)
                BottomBar
                    .ResizeHeightTo(BottomBarHeight, 500, Easing.OutElasticQuarter)
                    .ResizeWidthTo(SelectedTab.DrawWidth, 500, Easing.OutElasticQuarter)
                    .MoveTo(new Vector2(SelectedTab.DrawPosition.X, 0), 500, Easing.OutElasticQuarter);
            else
                BottomBar
                   .ResizeHeightTo(0, 500, Easing.OutElasticQuarter);
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new OverlayRulesetTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            AutoSizeAxes = Axes.Both,
            Direction = FillDirection.Horizontal,
            Spacing = new Vector2(20, 0),
            Anchor = Anchor.CentreLeft,
            Origin = Anchor.CentreLeft,
        };
    }
}
