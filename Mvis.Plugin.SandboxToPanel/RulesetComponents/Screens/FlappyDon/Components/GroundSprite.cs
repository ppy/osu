using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;

namespace Mvis.Plugin.SandboxToPanel.RulesetComponents.Screens.FlappyDon.Components
{
    public partial class GroundSprite : Sprite
    {
        private Vector2 textureSize;

        public GroundSprite()
        {
            Anchor = Anchor.BottomLeft;
            Origin = Anchor.BottomLeft;
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            Texture = textures.Get("FlappyDon/ground", WrapMode.ClampToBorder, WrapMode.ClampToBorder);
            textureSize = Texture.Size;
            Height = FlappyDonGame.GROUND_HEIGHT;
        }

        protected override void Update()
        {
            base.Update();
            Width = DrawHeight * textureSize.X / textureSize.Y;
        }
    }
}
