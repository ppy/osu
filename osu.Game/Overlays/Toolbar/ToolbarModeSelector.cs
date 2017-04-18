// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

namespace osu.Game.Overlays.Toolbar
{
    internal class ToolbarModeSelector : Container
    {
        private const float padding = 10;

        private readonly FillFlowContainer modeButtons;
        private readonly Drawable modeButtonLine;
        private ToolbarModeButton activeButton;

        public Action<RulesetInfo> OnRulesetChange;

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
        private void load(RulesetDatabase rulesets)
        {
            foreach (var ruleset in rulesets.AllRulesets)
            {
                modeButtons.Add(new ToolbarModeButton
                {
                    Ruleset = ruleset,
                    Action = delegate
                    {
                        SetRuleset(ruleset);
                        OnRulesetChange?.Invoke(ruleset);
                    }
                });
            }
        }

        protected override void Update()
        {
            base.Update();

            Size = new Vector2(modeButtons.DrawSize.X, 1);
        }

        public void SetRuleset(RulesetInfo ruleset)
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
