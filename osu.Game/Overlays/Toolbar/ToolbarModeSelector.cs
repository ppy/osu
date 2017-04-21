// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Configuration;

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarModeSelector : Container
    {
        private const float padding = 10;

        private readonly FillFlowContainer modeButtons;
        private readonly Drawable modeButtonLine;
        private ToolbarModeButton activeButton;

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
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = new Color4(255, 194, 224, 100),
                        Radius = 15,
                        Roundness = 15,
                    },
                    Children = new []
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
        private void load(RulesetDatabase rulesets, OsuGame game)
        {
            foreach (var r in rulesets.AllRulesets)
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
            ruleset.BindTo(game.Ruleset);
        }

        public override bool HandleInput => !ruleset.Disabled;

        private void disabledChanged(bool isDisabled) => FadeColour(isDisabled ? Color4.Gray : Color4.White, 300);

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

            if (!activeMode.EnsureValid())
                activeMode.Refresh(() => modeButtonLine.MoveToX(activeButton.DrawPosition.X, 200, EasingTypes.OutQuint));
        }
    }
}
