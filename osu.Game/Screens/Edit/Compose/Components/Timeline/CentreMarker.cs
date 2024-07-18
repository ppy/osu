// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class CentreMarker : CompositeDrawable
    {
        private const float triangle_width = 8;

        private const float bar_width = 1.6f;

        public CentreMarker()
        {
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(triangle_width, 1);

            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    Width = bar_width,
                    Blending = BlendingParameters.Additive,
                    Colour = ColourInfo.GradientVertical(colours.Colour2.Opacity(0.6f), colours.Colour2.Opacity(0)),
                },
                new Triangle
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.BottomCentre,
                    Size = new Vector2(triangle_width, triangle_width * 0.8f),
                    Scale = new Vector2(1, -1),
                    Colour = colours.Colour2,
                },
            };
        }
    }
}
