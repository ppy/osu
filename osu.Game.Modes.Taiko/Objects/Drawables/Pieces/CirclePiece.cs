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
    class CirclePiece : CircularContainer
    {
        private TrianglesPiece triangles;

        public CirclePiece()
        {
            Size = new Vector2(128);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                new Box()
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 1
                },
                triangles = new TrianglesPiece()
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.1f,
                    Colour = Color4.Black
                },
            };
        }
    }
}
