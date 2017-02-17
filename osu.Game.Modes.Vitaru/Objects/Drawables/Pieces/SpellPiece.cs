using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace osu.Game.Modes.Vitaru.Objects.Drawables.Pieces
{
    class SpellPiece : Container
    {
        private Sprite sprite;

        public float degreesPerSecond = 80;
        public float normalSize = 200;
        public float sineHeight = 100;
        public float sineSpeed = 0.001f;

        public SpellPiece()
        {
            sprite = new Sprite()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Alpha = 0.25f,
            };
            Add(sprite);
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            sprite.Texture = textures.Get(@"Play/Vitaru/sign");
        }

        protected override void Update()
        {
            base.Update();

            sprite.ResizeTo((float)Math.Abs(Math.Sin(Clock.CurrentTime * sineSpeed)) * sineHeight + normalSize);
            sprite.RotateTo((float)((Clock.CurrentTime / 1000) * degreesPerSecond));
        }
    }
}
