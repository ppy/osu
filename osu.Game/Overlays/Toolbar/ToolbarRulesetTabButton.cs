// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets;
using osuTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarRulesetTabButton : TabItem<RulesetInfo>
    {
        private readonly RulesetButton ruleset;

        public ToolbarRulesetTabButton(RulesetInfo value)
            : base(value)
        {
            AutoSizeAxes = Axes.X;
            RelativeSizeAxes = Axes.Y;
            Child = ruleset = new RulesetButton
            {
                Active = false,
            };

            var rInstance = value.CreateInstance();

            ruleset.TooltipMain = rInstance.Description;
            ruleset.TooltipSub = $"Play some {rInstance.Description}";
            ruleset.SetIcon(rInstance.CreateIcon());
        }

        protected override void OnActivated() => ruleset.Active = true;

        protected override void OnDeactivated() => ruleset.Active = false;

        private class RulesetButton : ToolbarButton
        {
            public bool Active
            {
                set
                {
                    if (value)
                    {
                        IconContainer.Colour = Color4.White;
                        IconContainer.EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Glow,
                            Colour = new Color4(255, 194, 224, 100),
                            Radius = 15,
                            Roundness = 15,
                        };
                    }
                    else
                    {
                        IconContainer.Colour = new Color4(255, 194, 224, 255);
                        IconContainer.EdgeEffect = new EdgeEffectParameters();
                    }
                }
            }

            protected override bool OnClick(ClickEvent e)
            {
                Parent.Click();
                return base.OnClick(e);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                IconContainer.Scale *= 1.4f;
            }
        }
    }
}
