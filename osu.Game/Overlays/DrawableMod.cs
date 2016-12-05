//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Modes;

namespace osu.Game.Overlays
{
    public class ModIcon : Container
    {
        private TextAwesome icon, bg;

        private Color4 bgcolor;
        public new Color4 Colour
        {
            get { return bgcolor; }
            set { bgcolor = value; bg.Colour = value; }
        }

        private FontAwesome iconName;
        public FontAwesome Icon
        {
            get { return iconName; }
            set { iconName = value; icon.Icon = value; }
        }

        public ModIcon()
        {
            Children = new Drawable[]
            {
                bg = new TextAwesome
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    TextSize = 80,
                    Icon = FontAwesome.fa_osu_mod_bg,
                    Shadow = true,
                },
                icon = new TextAwesome
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    TextSize = 50,
                    Colour = new Color4(84,84,84,255)
                }
            };
        }
    }
    public class DrawableMod : Container
    {
        private ModIcon icon;
        private SpriteText text;

        private Color4 bgcolor;
        public new Color4 Colour
        {
            get { return bgcolor; }
            set { bgcolor = value; icon.Colour = value; }
        }

        private Mod mod;
        public Mod Mod
        {
            get { return mod; }
            set
            {
                mod = value;
                icon.Icon = value.Icon;
                text.Text = Mod.Name.GetDescription();
            }
        }

        public DrawableMod()
        {
            Children = new Drawable[]
            {
                icon = new ModIcon
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                },
                text = new SpriteText
                {
                    Margin = new MarginPadding { Top = 15 },
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    TextSize = 18,
                }
            };
        }
    }
}
