using System;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class GraphicsOptions : OptionsSection
    {
        protected override string Header => "Graphics";
        public override FontAwesome Icon => FontAwesome.fa_laptop;
    
        public GraphicsOptions()
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

