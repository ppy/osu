using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.FlappyDon.Components
{
    public partial class PipeSprite : Sprite
    {
        public PipeSprite()
        {
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
            Scale = new Vector2(4.1f);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get("FlappyDon/pipe");
        }
    }
}
