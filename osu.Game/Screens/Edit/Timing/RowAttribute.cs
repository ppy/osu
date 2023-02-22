// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Edit.Timing
{
    public partial class RowAttribute : CompositeDrawable
    {
        protected readonly ControlPoint Point;

        private readonly string label;

        protected Drawable Background { get; private set; } = null!;

        protected FillFlowContainer Content { get; private set; } = null!;

        public RowAttribute(ControlPoint point, string label)
        {
            Point = point;

            this.label = label;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, OverlayColourProvider overlayColours)
        {
            AutoSizeAxes = Axes.X;

            Height = 20;

            Anchor = Anchor.CentreLeft;
            Origin = Anchor.CentreLeft;

            Masking = true;
            CornerRadius = 3;

            InternalChildren = new[]
            {
                Background = new Box
                {
                    Colour = overlayColours.Background5,
                    RelativeSizeAxes = Axes.Both,
                },
                Content = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Horizontal = 5 },
                    Spacing = new Vector2(5),
                    Children = new Drawable[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Colour = Point.GetRepresentingColour(colours),
                            RelativeSizeAxes = Axes.Y,
                            Size = new Vector2(4, 0.6f),
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Padding = new MarginPadding(3),
                            Font = OsuFont.Default.With(weight: FontWeight.Bold, size: 12),
                            Text = label,
                        },
                    },
                }
            };
        }
    }
}
