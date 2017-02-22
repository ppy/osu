using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    class GlowPiece : Container
    {
        private Sprite glow;

        public GlowPiece()
        {
            RelativeSizeAxes = Axes.Both;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new[]
            {
                glow = new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,

                    BlendingMode = BlendingMode.Additive,
                    Alpha = 0.5f
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            glow.Texture = textures.Get(@"Play/Taiko/ring-glow");
        }
    }

}
