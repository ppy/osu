using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class GraphicsOptions : OptionsSection
    {
        protected override string Header => "Graphics";
    
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

