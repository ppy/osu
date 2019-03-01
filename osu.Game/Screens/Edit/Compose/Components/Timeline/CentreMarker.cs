// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public class CentreMarker : CompositeDrawable
    {
        private const float triangle_width = 20;
        private const float triangle_height = 10;
        private const float bar_width = 2;

        public CentreMarker()
        {
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(20, 1);

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Width = bar_width,
                },
                new Triangle
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(triangle_width, triangle_height),
                    Scale = new Vector2(1, -1)
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Red;
        }
    }
}
