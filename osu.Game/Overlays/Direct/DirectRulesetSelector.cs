// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Direct
{
    public class DirectRulesetSelector : RulesetSelector
    {
        public override bool HandleNonPositionalInput => !Current.Disabled && base.HandleNonPositionalInput;

        public override bool HandlePositionalInput => !Current.Disabled && base.HandlePositionalInput;

        public override bool PropagatePositionalInputSubTree => !Current.Disabled && base.PropagatePositionalInputSubTree;

        public DirectRulesetSelector()
        {
            TabContainer.Masking = false;
            TabContainer.Spacing = new Vector2(10, 0);
            AutoSizeAxes = Axes.Both;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Current.BindDisabledChanged(value => SelectedTab.FadeColour(value ? Color4.DarkGray : Color4.White, 200, Easing.OutQuint), true);
        }

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new DirectRulesetTabItem(value);

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            Direction = FillDirection.Horizontal,
            AutoSizeAxes = Axes.Both,
        };

        private class DirectRulesetTabItem : TabItem<RulesetInfo>
        {
            private readonly ConstrainedIconContainer iconContainer;

            public DirectRulesetTabItem(RulesetInfo value)
                : base(value)
            {
                AutoSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    iconContainer = new ConstrainedIconContainer
                    {
                        Icon = value.CreateInstance().CreateIcon(),
                        Size = new Vector2(32),
                    },
                    new HoverClickSounds()
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                updateState();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                updateState();
            }

            protected override void OnActivated() => updateState();

            protected override void OnDeactivated() => updateState();

            private void updateState() => iconContainer.FadeColour(IsHovered || Active.Value ? Color4.White : Color4.Gray, 120, Easing.InQuad);
        }
    }
}
