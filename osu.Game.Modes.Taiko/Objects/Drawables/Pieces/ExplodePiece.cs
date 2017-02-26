using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace osu.Game.Modes.Taiko.Objects.Drawables.Pieces
{
    public class FinisherExplodePiece : ExplodePiece
    {
        public FinisherExplodePiece()
        {
            Size *= 1.5f;
        }
    }

    public class ExplodePiece : CircularContainer
    {
        public ExplodePiece()
        {
            Size = new Vector2(128);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Alpha = 0;

            Children = new[]
            {
                new Box()
                {
                    BlendingMode = BlendingMode.Additive,
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0.2f
                }
            };

            BorderColour = Color4.White;
            BorderThickness = 1;
        }
    }
}
