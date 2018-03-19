using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Vitaru.Objects.Characters.Pieces
{
    public class Rift : Sprite
    {
        public Rift LinkedRift;

        public Rift(Color4 color)
        {
            AlwaysPresent = true;

            Anchor = Framework.Graphics.Anchor.TopLeft;
            Origin = Framework.Graphics.Anchor.Centre;

            Alpha = 0;
            Colour = color;
            Size = new Vector2(80);
            Texture = VitaruRuleset.VitaruTextures.Get("vortex");
        }

        protected override void Update()
        {
            base.Update();

            Rotation = (float)(Clock.CurrentTime / -1000 * 90);
        }
    }
}
