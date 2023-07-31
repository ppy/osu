// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class RectangularPositionSnapGrid : CompositeDrawable
    {
        /// <summary>
        /// The position of the origin of this <see cref="RectangularPositionSnapGrid"/> in local coordinates.
        /// </summary>
        public Vector2 StartPosition { get; }

        private Vector2 spacing = Vector2.One;

        /// <summary>
        /// The spacing between grid lines of this <see cref="RectangularPositionSnapGrid"/>.
        /// </summary>
        public Vector2 Spacing
        {
            get => spacing;
            set
            {
                if (spacing.X <= 0 || spacing.Y <= 0)
                    throw new ArgumentException("Grid spacing must be positive.");

                spacing = value;
                gridCache.Invalidate();
            }
        }

        private readonly LayoutValue gridCache = new LayoutValue(Invalidation.RequiredParentSizeToFit);

        public RectangularPositionSnapGrid(Vector2 startPosition)
        {
            StartPosition = startPosition;

            AddLayout(gridCache);
        }

        protected override void Update()
        {
            base.Update();

            if (!gridCache.IsValid)
            {
                ClearInternal();

                if (DrawWidth > 0 && DrawHeight > 0)
                    createContent();

                gridCache.Validate();
            }
        }

        private void createContent()
        {
            var drawSize = DrawSize;

            generateGridLines(Direction.Horizontal, StartPosition.Y, 0, -Spacing.Y);
            generateGridLines(Direction.Horizontal, StartPosition.Y, drawSize.Y, Spacing.Y);

            generateGridLines(Direction.Vertical, StartPosition.X, 0, -Spacing.X);
            generateGridLines(Direction.Vertical, StartPosition.X, drawSize.X, Spacing.X);
        }

        private void generateGridLines(Direction direction, float startPosition, float endPosition, float step)
        {
            int index = 0;
            float currentPosition = startPosition;

            // Make lines the same width independent of display resolution.
            float lineWidth = DrawWidth / ScreenSpaceDrawQuad.Width;

            List<Box> generatedLines = new List<Box>();

            while (Precision.AlmostBigger((endPosition - currentPosition) * Math.Sign(step), 0))
            {
                var gridLine = new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.1f,
                };

                if (direction == Direction.Horizontal)
                {
                    gridLine.Origin = Anchor.CentreLeft;
                    gridLine.RelativeSizeAxes = Axes.X;
                    gridLine.Height = lineWidth;
                    gridLine.Y = currentPosition;
                }
                else
                {
                    gridLine.Origin = Anchor.TopCentre;
                    gridLine.RelativeSizeAxes = Axes.Y;
                    gridLine.Width = lineWidth;
                    gridLine.X = currentPosition;
                }

                generatedLines.Add(gridLine);

                index += 1;
                currentPosition = startPosition + index * step;
            }

            if (generatedLines.Count == 0)
                return;

            generatedLines.First().Alpha = 0.3f;
            generatedLines.Last().Alpha = 0.3f;

            AddRangeInternal(generatedLines);
        }

        public Vector2 GetSnappedPosition(Vector2 original)
        {
            Vector2 relativeToStart = original - StartPosition;
            Vector2 offset = Vector2.Divide(relativeToStart, Spacing);
            Vector2 roundedOffset = new Vector2(MathF.Round(offset.X), MathF.Round(offset.Y));

            return StartPosition + Vector2.Multiply(roundedOffset, Spacing);
        }
    }
}
