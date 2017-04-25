// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.UI
{
    public class ModIcon : Container
    {
        private readonly TextAwesome modIcon;
        private readonly TextAwesome background;

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

        public new Color4 Colour
        {
            get
            {
                return background.Colour;
            }
            set
            {
                background.Colour = value;
            }
        }

        public FontAwesome Icon
        {
            get
            {
                return modIcon.Icon;
            }
            set
            {
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
                    TextSize = 20
                },
                modIcon = new TextAwesome
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = OsuColour.Gray(84),
                    TextSize = 20
                },
            };

            reapplySize();
        }
    }
}