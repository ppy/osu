// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Game.Overlays;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Screens.Play
{
    public class SongProgressBar : DragBar
    {
        public Color4 FillColour
        {
            get { return Fill.Colour; }
            set { Fill.Colour = value; }
        }

        public SongProgressBar(float barHeight, float handleBarHeight, Vector2 handleSize)
        {
            Height = barHeight + handleBarHeight + handleSize.Y;

            Fill.RelativeSizeAxes = Axes.X;
            Fill.Height = barHeight;

            Add(new Box
            {
                Name = "Background",
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.X,
                Height = barHeight,
                Colour = Color4.Black,
                Alpha = 0.5f,
                Depth = 1
            });

            Fill.Add(new Container
            {
                Origin = Anchor.BottomRight,
                Anchor = Anchor.BottomRight,
                Width = 2,
                Height = barHeight + handleBarHeight,
                Colour = Color4.White,
                Position = new Vector2(2, 0),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    },
                    new Container
                    {
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.TopCentre,
                        Size = handleSize,
                        CornerRadius = 5,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White
                            }
                        }
                    }
                }
            });
        }
    }
}
