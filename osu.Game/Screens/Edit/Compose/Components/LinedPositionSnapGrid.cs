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
    public abstract partial class LinedPositionSnapGrid : CompositeDrawable
    {
        private Vector2 startPosition;

        /// <summary>
        /// The position of the origin of this <see cref="TriangularPositionSnapGrid"/> in local coordinates.
        /// </summary>
        public Vector2 StartPosition
        {
            get => startPosition;
            set
            {
                startPosition = value;
                GridCache.Invalidate();
            }
        }

        protected readonly LayoutValue GridCache = new LayoutValue(Invalidation.RequiredParentSizeToFit);

        protected LinedPositionSnapGrid()
        {
            Masking = true;

            AddLayout(GridCache);
        }

        protected override void Update()
        {
            base.Update();

            if (!GridCache.IsValid)
            {
                ClearInternal();

                if (DrawWidth > 0 && DrawHeight > 0)
                    CreateContent();

                GridCache.Validate();
            }
        }

        protected abstract void CreateContent();

        protected void GenerateGridLines(Vector2 step, Vector2 drawSize)
        {
            int index = 0;
            var currentPosition = startPosition;

            // Make lines the same width independent of display resolution.
            float lineWidth = DrawWidth / ScreenSpaceDrawQuad.Width;
            float lineLength = drawSize.Length * 2;

            List<Box> generatedLines = new List<Box>();

            while (lineDefinitelyIntersectsBox(currentPosition, step.PerpendicularLeft, drawSize) ||
                   isMovingTowardsBox(currentPosition, step, drawSize))
            {
                var gridLine = new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.1f,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Width = lineWidth,
                    Height = lineLength,
                    Position = currentPosition,
                    Rotation = MathHelper.RadiansToDegrees(MathF.Atan2(step.Y, step.X)),
                };

                generatedLines.Add(gridLine);

                index += 1;
                currentPosition = startPosition + index * step;
            }

            if (generatedLines.Count == 0)
                return;

            generatedLines.First().Alpha = 0.3f;

            AddRangeInternal(generatedLines);
        }

        private bool isMovingTowardsBox(Vector2 currentPosition, Vector2 step, Vector2 box)
        {
            return (currentPosition + step).LengthSquared < currentPosition.LengthSquared ||
                   (currentPosition + step - box).LengthSquared < (currentPosition - box).LengthSquared;
        }

        private bool lineDefinitelyIntersectsBox(Vector2 lineStart, Vector2 lineDir, Vector2 box)
        {
            var p2 = lineStart + lineDir;

            double d1 = det(Vector2.Zero);
            double d2 = det(new Vector2(box.X, 0));
            double d3 = det(new Vector2(0, box.Y));
            double d4 = det(box);

            return definitelyDifferentSign(d1, d2) || definitelyDifferentSign(d3, d4) ||
                   definitelyDifferentSign(d1, d3) || definitelyDifferentSign(d2, d4);

            double det(Vector2 p) => (p.X - lineStart.X) * (p2.Y - lineStart.Y) - (p.Y - lineStart.Y) * (p2.X - lineStart.X);

            bool definitelyDifferentSign(double a, double b) => !Precision.AlmostEquals(a, 0) &&
                                                                !Precision.AlmostEquals(b, 0) &&
                                                                Math.Sign(a) != Math.Sign(b);
        }

        protected void GenerateOutline(Vector2 drawSize)
        {
            // Make lines the same width independent of display resolution.
            float lineWidth = DrawWidth / ScreenSpaceDrawQuad.Width;

            AddRangeInternal(new[]
            {
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = lineWidth,
                    Y = 0,
                },
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.CentreLeft,
                    RelativeSizeAxes = Axes.X,
                    Height = lineWidth,
                    Y = drawSize.Y,
                },
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    Width = lineWidth,
                    X = 0,
                },
                new Box
                {
                    Colour = Colour4.White,
                    Alpha = 0.3f,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.Y,
                    Width = lineWidth,
                    X = drawSize.X,
                },
            });
        }

        public abstract Vector2 GetSnappedPosition(Vector2 original);
    }
}
