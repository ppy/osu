using System;
using osu.Framework.Graphics.Transformations;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SpinnerBottom : CircularContainer
    {
        private Box circleBox;
        public SpinnerBottom(Spinner s)
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 1f;
            Masking = true;
            Children = new Drawable[]
            {
                circleBox = new Box
                {
                    Size = new Vector2(200),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.Black,
                    Alpha = 0.52f,
                }
            };
        }
    }
}
