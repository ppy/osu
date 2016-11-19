using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class ExplodePiece : Container
    {
        public ExplodePiece()
        {
            Size = new Vector2(144);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            BlendingMode = BlendingMode.Additive;
            Alpha = 0;

            Children = new Drawable[]
            {
                new Triangles
                {
                    BlendingMode = BlendingMode.Additive,
                    RelativeSizeAxes = Axes.Both
                }
            };
        }
    }
}