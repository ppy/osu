// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using osuTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarRulesetButton : ToolbarButton
    {
        private RulesetInfo ruleset;

        public RulesetInfo Ruleset
        {
            get => ruleset;
            set
            {
                ruleset = value;

                var rInstance = ruleset.CreateInstance();

                TooltipMain = rInstance.Description;
                TooltipSub = $"Play some {rInstance.Description}";
                SetIcon(rInstance.CreateIcon());
            }
        }

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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            IconContainer.Scale *= 1.4f;
        }
    }
}
