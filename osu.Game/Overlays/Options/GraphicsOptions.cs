using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class GraphicsOptions : OptionsSection
    {
        public GraphicsOptions()
        {
            Header = "Graphics";
            Children = new Drawable[]
            {
                new RendererOptions(),
                new LayoutOptions(),
                new DetailSettings(),
                new MainMenuOptions(),
                new SongSelectGraphicsOptions(),
            };
        }
    }
}

