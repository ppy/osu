//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.Graphics
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
                new SongSelectGraphicsOptions(),
            };
        }
    }
}

