// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.Break
{
    public partial class LetterboxOverlay : CompositeDrawable
    {
        private const int height = 350;

        private static readonly Color4 transparent_black = new Color4(0, 0, 0, 0);

        public LetterboxOverlay()
        {
            RelativeSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    Colour = ColourInfo.GradientVertical(Color4.Black, transparent_black),
                },
                new Box
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = height,
                    Colour = ColourInfo.GradientVertical(transparent_black, Color4.Black),
                }
            };
        }
    }
}
