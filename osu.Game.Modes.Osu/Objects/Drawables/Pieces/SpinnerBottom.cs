using System;
using osu.Framework.Graphics.Transformations;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Graphics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class SpinnerBottom : CircularContainer
    {
        private Box circleBox;
        private CircularContainer innerCircleContainer;
        public SpinnerBottom(Spinner s)
        {
            AutoSizeAxes = Axes.Both;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Alpha = 1f;
            Children = new Drawable[]
            {
                circleBox = new Box
                {
                    Size = new Vector2(200),
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Colour = Color4.Black,
                    Alpha = 0.52f,
                },
                innerCircleContainer = new CircularContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Alpha = 1f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Size = new Vector2(144),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Colour = s.Colour,
                            Alpha = 0.31f,
                        }
                    }
                }
            };
        }

        protected override void Update()
        {
            base.Update();
            innerCircleContainer.ScaleTo(0.7f, 50);
        }
    }
}
