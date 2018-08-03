// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using OpenTK;

namespace osu.Game.Rulesets.UI
{
    public class ModIcon : Container, IHasTooltip
    {
        private readonly SpriteIcon modIcon;
        private readonly SpriteIcon background;

        private const float size = 80;

        public FontAwesome Icon
        {
            get { return modIcon.Icon; }
            set { modIcon.Icon = value; }
        }

        private readonly ModType type;

        public virtual string TooltipText { get; }

        public ModIcon(Mod mod)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));

            type = mod.Type;

            TooltipText = mod.Name;

            Size = new Vector2(size);

            Children = new Drawable[]
            {
                background = new SpriteIcon
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(size),
                    Icon = FontAwesome.fa_osu_mod_bg,
                    Y = -6.5f,
                    Shadow = true,
                },
                modIcon = new SpriteIcon
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = OsuColour.Gray(84),
                    Size = new Vector2(size - 35),
                    Icon = mod.Icon
                },
            };
        }

        private Color4 backgroundColour;
        private Color4 highlightedColour;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            switch (type)
            {
                default:
                case ModType.DifficultyIncrease:
                    backgroundColour = colours.Yellow;
                    highlightedColour = colours.YellowLight;
                    break;
                case ModType.DifficultyReduction:
                    backgroundColour = colours.Green;
                    highlightedColour = colours.GreenLight;
                    break;
                case ModType.Automation:
                    backgroundColour = colours.Blue;
                    highlightedColour = colours.BlueLight;
                    break;
                case ModType.Conversion:
                    backgroundColour = colours.Purple;
                    highlightedColour = colours.PurpleLight;
                    break;
                case ModType.Fun:
                    backgroundColour = colours.Pink;
                    highlightedColour = colours.PinkLight;
                    break;
            }

            applyStyle();
        }

        private bool highlighted;

        public bool Highlighted
        {
            get { return highlighted; }

            set
            {
                highlighted = value;
                applyStyle();
            }
        }

        private void applyStyle()
        {
            background.Colour = highlighted ? highlightedColour : backgroundColour;
        }
    }
}
