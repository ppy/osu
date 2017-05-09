﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.UI
{
    public class ModIcon : Container
    {
        private readonly TextAwesome modIcon;
        private readonly TextAwesome background;

        private const float icon_size = 80;

        public FontAwesome Icon
        {
            get { return modIcon.Icon; }
            set { modIcon.Icon = value; }
        }

        private readonly ModType type;

        public ModIcon(Mod mod)
        {
            if (mod == null) throw new ArgumentNullException(nameof(mod));

            type = mod.Type;

            Children = new Drawable[]
            {
                background = new TextAwesome
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    TextSize = icon_size,
                    Icon = FontAwesome.fa_osu_mod_bg,
                    Shadow = true,
                },
                modIcon = new TextAwesome
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Colour = OsuColour.Gray(84),
                    TextSize = icon_size - 35,
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
                case ModType.Special:
                    backgroundColour = colours.Blue;
                    highlightedColour = colours.BlueLight;
                    break;
            }

            applyStyle();
        }

        private bool highlighted;

        public bool Highlighted
        {
            get
            {
                return highlighted;
            }

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
