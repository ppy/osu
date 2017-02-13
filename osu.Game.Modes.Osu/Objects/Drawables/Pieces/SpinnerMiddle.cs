using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SpinnerMiddle : CircularContainer
    {
        private Box innerCircleBox;

        public SpinnerMiddle(Spinner s)
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 0.52f;
            Masking = true;
            Children = new Drawable[]
            {
                innerCircleBox = new Box
                {
                    Size = new Vector2(144),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = s.Colour,
                    Alpha = 0.83f,
                }
            };
        }
    }
}
