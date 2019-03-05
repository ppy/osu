// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using osuTK.Input;
using osuTK.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarRulesetSelector : Container
    {
        private const float padding = 10;

        private readonly FillFlowContainer modeButtons;
        private readonly Drawable modeButtonLine;
        private ToolbarRulesetButton activeButton;

        private RulesetStore rulesets;
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        public ToolbarRulesetSelector()
        {
            RelativeSizeAxes = Axes.Y;

            Children = new[]
            {
                new OpaqueBackground(),
                modeButtons = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding { Left = padding, Right = padding },
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
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(RulesetStore rulesets, Bindable<RulesetInfo> parentRuleset)
        {
            this.rulesets = rulesets;
            foreach (var r in rulesets.AvailableRulesets)
            {
                modeButtons.Add(new ToolbarRulesetButton
                {
                    Ruleset = r,
                    Action = delegate { ruleset.Value = r; }
                });
            }

            ruleset.ValueChanged += rulesetChanged;
            ruleset.DisabledChanged += disabledChanged;
            ruleset.BindTo(parentRuleset);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            base.OnKeyDown(e);

            if (e.ControlPressed && !e.Repeat && e.Key >= Key.Number1 && e.Key <= Key.Number9)
            {
                int requested = e.Key - Key.Number1;

                RulesetInfo found = rulesets.AvailableRulesets.Skip(requested).FirstOrDefault();
                if (found != null)
                    ruleset.Value = found;
                return true;
            }

            return false;
        }

        public override bool HandleNonPositionalInput => !ruleset.Disabled && base.HandleNonPositionalInput;
        public override bool HandlePositionalInput => !ruleset.Disabled && base.HandlePositionalInput;

        public override bool PropagatePositionalInputSubTree => !ruleset.Disabled && base.PropagatePositionalInputSubTree;

        private void disabledChanged(bool isDisabled) => this.FadeColour(isDisabled ? Color4.Gray : Color4.White, 300);

        protected override void Update()
        {
            base.Update();
            Size = new Vector2(modeButtons.DrawSize.X, 1);
        }

        private void rulesetChanged(ValueChangedEvent<RulesetInfo> e)
        {
            foreach (ToolbarRulesetButton m in modeButtons.Children.Cast<ToolbarRulesetButton>())
            {
                bool isActive = m.Ruleset.ID == e.NewValue.ID;
                m.Active = isActive;
                if (isActive)
                    activeButton = m;
            }

            activeMode.Invalidate();
        }

        private Cached activeMode = new Cached();

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (!activeMode.IsValid)
            {
                modeButtonLine.MoveToX(activeButton.DrawPosition.X, 200, Easing.OutQuint);
                activeMode.Validate();
            }
        }
    }
}
