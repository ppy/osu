// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class TopPointPiece : CompositeDrawable
    {
        protected readonly ControlPoint Point;

        protected OsuSpriteText Label { get; private set; } = null!;

        public const float WIDTH = 80;

        public TopPointPiece(ControlPoint point)
        {
            Point = point;
            Width = WIDTH;
            Height = 16;
            Margin = new MarginPadding { Vertical = 4 };

            Origin = Anchor.TopCentre;
            Anchor = Anchor.TopCentre;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            const float corner_radius = 4;
            const float arrow_extension = 3;
            const float triangle_portion = 15;

            InternalChildren = new Drawable[]
            {
                // This is a triangle, trust me.
                // Doing it this way looks okay. Doing it using Triangle primitive is basically impossible.
                new Container
                {
                    Colour = Point.GetRepresentingColour(colours),
                    X = -corner_radius,
                    Size = new Vector2(triangle_portion * arrow_extension, Height),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Masking = true,
                    CornerRadius = Height,
                    CornerExponent = 1.4f,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                        },
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = WIDTH - triangle_portion,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Colour = Point.GetRepresentingColour(colours),
                    Masking = true,
                    CornerRadius = corner_radius,
                    Child = new Box
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                    },
                },
                Label = new OsuSpriteText
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding(3),
                    Font = OsuFont.Default.With(size: 14, weight: FontWeight.SemiBold),
                    Colour = colours.B5,
                }
            };
        }
    }
}
