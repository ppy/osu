// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Layout;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class RectangularPositionSnapGrid : CompositeDrawable
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

            while ((endPosition - currentPosition) * Math.Sign(step) > 0)
            {
                var gridLine = new Box
                {
                    Colour = Colour4.White,
                    Alpha = index == 0 ? 0.3f : 0.1f,
                    EdgeSmoothness = new Vector2(0.2f)
                };

                if (direction == Direction.Horizontal)
                {
                    gridLine.RelativeSizeAxes = Axes.X;
                    gridLine.Height = 1;
                    gridLine.Y = currentPosition;
                }
                else
                {
                    gridLine.RelativeSizeAxes = Axes.Y;
                    gridLine.Width = 1;
                    gridLine.X = currentPosition;
                }

                AddInternal(gridLine);

                index += 1;
                currentPosition = startPosition + index * step;
            }
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
