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
using osu.Framework.Extensions.Color4Extensions;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    class CirclePiece : Container
    {
        private Sprite disc;

        private TrianglesPiece triangles;

        public CirclePiece()
        {
            Size = new Vector2(64);

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
                triangles = new TrianglesPiece()
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.05f,
                    Colour = Color4.Black
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            disc.Texture = textures.Get(@"Play/Taiko/disc");
        }
    }
}
