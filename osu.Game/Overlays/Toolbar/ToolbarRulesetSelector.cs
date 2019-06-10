// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osuTK.Input;
using System.Linq;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarRulesetSelector : TabControl<RulesetInfo>
    {
        private const float padding = 10;
        private readonly Drawable modeButtonLine;
        private RulesetStore rulesets;

        public override bool HandleNonPositionalInput => !Current.Disabled && base.HandleNonPositionalInput;
        public override bool HandlePositionalInput => !Current.Disabled && base.HandlePositionalInput;

        public override bool PropagatePositionalInputSubTree => !Current.Disabled && base.PropagatePositionalInputSubTree;

        private void disabledChanged(bool isDisabled) => this.FadeColour(isDisabled ? Color4.Gray : Color4.White, 300);

        protected override Dropdown<RulesetInfo> CreateDropdown() => null;

        protected override TabItem<RulesetInfo> CreateTabItem(RulesetInfo value) => new ToolbarRulesetButton(value);

        public ToolbarRulesetSelector()
        {
            RelativeSizeAxes = Axes.Y;
            AutoSizeAxes = Axes.X;

            AddRangeInternal(new[]
            {
                new OpaqueBackground
                {
                    Depth = 1,
                },
                modeButtonLine = new Container
                {
                    Size = new Vector2(padding * 2 + ToolbarButton.WIDTH, 3),
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.TopLeft,
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = new Color4(255, 194, 224, 100),
                        Radius = 15,
                        Roundness = 15,
                    },
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            });
        }

        protected override TabFillFlowContainer CreateTabFlow() => new TabFillFlowContainer
        {
            RelativeSizeAxes = Axes.Y,
            AutoSizeAxes = Axes.X,
            Direction = FillDirection.Horizontal,
            Padding = new MarginPadding { Left = padding, Right = padding },
        };

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets, Bindable<RulesetInfo> parentRuleset)
        {
            this.rulesets = rulesets;

            foreach (var r in rulesets.AvailableRulesets)
            {
                AddItem(r);
            }

            Current.BindTo(parentRuleset);
            Current.DisabledChanged += disabledChanged;
            Current.BindValueChanged(rulesetChanged);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            base.OnKeyDown(e);

            if (e.ControlPressed && !e.Repeat && e.Key >= Key.Number1 && e.Key <= Key.Number9)
            {
                int requested = e.Key - Key.Number1;

                RulesetInfo found = rulesets.AvailableRulesets.Skip(requested).FirstOrDefault();
                if (found != null)
                    Current.Value = found;
                return true;
            }

            return false;
        }

        private readonly Cached activeMode = new Cached();

        private void rulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            activeMode.Invalidate();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!activeMode.IsValid)
            {
                foreach (TabItem<RulesetInfo> tabItem in TabContainer)
                {
                    if (tabItem.Value == Current.Value)
                    {
                        modeButtonLine.MoveToX(tabItem.DrawPosition.X, 200, Easing.OutQuint);
                        activeMode.Validate();
                        return;
                    }
                }
            }
        }
    }
}
