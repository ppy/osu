﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Overlays.Options.Sections.Graphics;

namespace osu.Game.Overlays.Options.Sections
{
    public class GraphicsSection : OptionsSection
    {
        public override string Header => "Graphics";
        public override FontAwesome Icon => FontAwesome.fa_laptop;

        public GraphicsSection()
        {
            Children = new Drawable[]
            {
                new RendererOptions(),
                new LayoutOptions(),
                new DetailOptions(),
                new MainMenuOptions(),
            };
        }
    }
}

