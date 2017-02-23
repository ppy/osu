using OpenTK;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    class CirclePiece : Container
    {
        private Sprite disc;

        public CirclePiece()
        {
            Size = new Vector2(128);

            Masking = true;
            CornerRadius = Size.X / 2;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                disc = new Sprite()
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                },
                new TrianglesPiece()
                {
                    RelativeSizeAxes = Axes.Both,

                    Colour = Color4.Black,
                    BlendingMode = BlendingMode.Mixture,
                    Alpha = 0.1f
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            disc.Texture = textures.Get(@"Play/Taiko/disc");
        }
    }

}
