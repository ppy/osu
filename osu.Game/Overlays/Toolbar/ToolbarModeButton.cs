// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;
using osu.Game.Modes;
using OpenTK.Graphics;

namespace osu.Game.Overlays.Toolbar
{
    public class ToolbarModeButton : ToolbarButton
    {
        private PlayMode mode;
        public PlayMode Mode
        {
            get { return mode; }
            set
            {
                mode = value;
                TooltipMain = Ruleset.GetRuleset(mode).Description;
                TooltipSub = $"Play some {Ruleset.GetRuleset(mode).Description}";
                Icon = Ruleset.GetRuleset(mode).Icon;
            }
        }

        public bool Active
        {
            set
            {
                if (value)
                {
                    DrawableIcon.Colour = Color4.White;
                    DrawableIcon.Masking = true;
                    DrawableIcon.EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = new Color4(255, 194, 224, 100),
                        Radius = 15,
                        Roundness = 15,
                    };
                }
                else
                {
                    DrawableIcon.Masking = false;
                    DrawableIcon.Colour = new Color4(255, 194, 224, 255);
                }
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DrawableIcon.TextSize *= 1.4f;
        }
    }
}