// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarModeSelector : KeyBindingContainer<int>
    {
        private const float padding = 10;

        private readonly FillFlowContainer modeButtons;
        private readonly Drawable modeButtonLine;
        private ToolbarModeButton activeButton;

        private int rulesetCount;
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();

        public ToolbarModeSelector()
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

        public override IEnumerable<osu.Framework.Input.Bindings.KeyBinding> DefaultKeyBindings
        {
            get
            {
                var keybinds = new List<osu.Framework.Input.Bindings.KeyBinding>();
                for (int i = 0; i < Math.Min(rulesetCount, 10); i++)
                {
                    InputKey numberKey;
                    if (i == 9)
                        numberKey = InputKey.Number0;
                    else
                        numberKey = (InputKey)i + 110;

                    keybinds.Add(new osu.Framework.Input.Bindings.KeyBinding(new[] { InputKey.Control, numberKey }, i));
                }
                return keybinds;
            }
        }

        private class RulesetSwitcherInputHandler : Container, IKeyBindingHandler<int>
        {
            private Bindable<RulesetInfo> ruleset;
            private RulesetStore rulesets;

            public RulesetSwitcherInputHandler(Bindable<RulesetInfo> ruleset, RulesetStore rulesets)
            {
                this.ruleset = ruleset;
                this.rulesets = rulesets;
            }

            public bool OnPressed(int action)
            {
                ruleset.Value = rulesets.GetRuleset(action);

                return true;
            }

            public bool OnReleased(int action) => false;
        }

        [BackgroundDependencyLoader(true)]
        private void load(RulesetStore rulesets, OsuGame game)
        {
            this.rulesetCount = rulesets.AvailableRulesets.Count();
            foreach (var r in rulesets.AvailableRulesets)
            {
                modeButtons.Add(new ToolbarModeButton
                {
                    Ruleset = r,
                    Action = delegate
                    {
                        ruleset.Value = r;
                    }
                });
            }

            ruleset.ValueChanged += rulesetChanged;
            ruleset.DisabledChanged += disabledChanged;
            if (game != null)
                ruleset.BindTo(game.Ruleset);
            else
                ruleset.Value = rulesets.AvailableRulesets.FirstOrDefault();

            Add(new RulesetSwitcherInputHandler(ruleset, rulesets));
        }

        public override bool HandleKeyboardInput => !ruleset.Disabled && base.HandleKeyboardInput;
        public override bool HandleMouseInput => !ruleset.Disabled && base.HandleMouseInput;

        private void disabledChanged(bool isDisabled) => this.FadeColour(isDisabled ? Color4.Gray : Color4.White, 300);

        protected override void Update()
        {
            base.Update();
            Size = new Vector2(modeButtons.DrawSize.X, 1);
        }

        private void rulesetChanged(RulesetInfo ruleset)
        {
            foreach (ToolbarModeButton m in modeButtons.Children.Cast<ToolbarModeButton>())
            {
                bool isActive = m.Ruleset.ID == ruleset.ID;
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
