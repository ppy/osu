// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class CentreMarker : CompositeDrawable
    {
        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            const float triangle_width = 8;
            const float bar_width = 2f;

            RelativeSizeAxes = Axes.Y;

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;

            Size = new Vector2(triangle_width, 1);

            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Width = bar_width,
                    Colour = colours.Colour2,
                },
                new Triangle
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(triangle_width, triangle_width * 0.8f),
                    Scale = new Vector2(1, -1),
                    EdgeSmoothness = new Vector2(1, 0),
                    Colour = colours.Colour2,
                },
                new Triangle
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(triangle_width, triangle_width * 0.8f),
                    Scale = new Vector2(1, 1),
                    Colour = colours.Colour2,
                },
            };
        }
    }
}
