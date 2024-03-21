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
                    Size = new Vector2(triangle_width, triangle_width * 0.8f),
                    Scale = new Vector2(1, -1)
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            Colour = colours.Red1;
        }
    }
}
