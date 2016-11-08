using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.Graphics
{
    public class GraphicsSection : OptionsSection
    {
        protected override string Header => "Graphics";
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

