// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarModeButton : ToolbarButton
    {
        private RulesetInfo ruleset;
        public RulesetInfo Ruleset
        {
            get { return ruleset; }
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
