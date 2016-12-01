//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
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
                TooltipMain = mode.GetDescription();
                TooltipSub = $"Play some {mode.GetDescription()}";
                Icon = getModeIcon(mode);
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

        private FontAwesome getModeIcon(PlayMode mode)
        {
            switch (mode)
            {
                default: return FontAwesome.fa_osu_osu_o;
                case PlayMode.Taiko: return FontAwesome.fa_osu_taiko_o;
                case PlayMode.Catch: return FontAwesome.fa_osu_fruits_o;
                case PlayMode.Mania: return FontAwesome.fa_osu_mania_o;
            }
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DrawableIcon.TextSize *= 1.4f;
        }
    }
}