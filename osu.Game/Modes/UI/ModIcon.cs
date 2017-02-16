// Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;

namespace osu.Game.Modes.UI
{
    public class ModIcon : Container
    {
        private TextAwesome modIcon, background;

        private float iconSize = 80;
        public float IconSize
        {
            get
            {
                return iconSize;
            }
            set
            {
                iconSize = value;
                reapplySize();
            }
        }

        private Color4 backgroundColour;
        new public Color4 Colour
        {
            get
            {
                return backgroundColour;
            }
            set
            {
                backgroundColour = value;
                background.Colour = value;
            }
        }

        private FontAwesome icon;
        public FontAwesome Icon
        {
            get
            {
                return icon;
            }
            set
            {
                icon = value;
                modIcon.Icon = value;
            }
        }

        private void reapplySize()
        {
            background.TextSize = iconSize;
            modIcon.TextSize = iconSize - 35;
        }

        public ModIcon()
        {
            Children = new Drawable[]
            {
                background = new TextAwesome
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Icon = FontAwesome.fa_osu_mod_bg,
                    Shadow = true,
                },
                modIcon = new TextAwesome
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = OsuColour.Gray(84),
                },
            };

            reapplySize();
        }
    }
}