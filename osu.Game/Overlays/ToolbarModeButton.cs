//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions;
using osu.Game.Graphics;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Game.Modes;
using osu.Game.Screens.Play;

namespace osu.Game.Overlays
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
                //Background.Colour = value ? new Color4(100, 100, 100, 255) : new Color4(20, 20, 20, 255);
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