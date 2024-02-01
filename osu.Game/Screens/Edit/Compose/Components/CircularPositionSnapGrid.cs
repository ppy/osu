// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class CircularPositionSnapGrid : PositionSnapGrid
    {
        /// <summary>
        /// The spacing between grid lines of this <see cref="CircularPositionSnapGrid"/>.
        /// </summary>
        public BindableFloat Spacing { get; } = new BindableFloat(1f)
        {
            MinValue = 0f,
        };

        public CircularPositionSnapGrid()
        {
            Spacing.BindValueChanged(_ => GridCache.Invalidate());
        }

        protected override void CreateContent()
        {
            var drawSize = DrawSize;

            // Calculate the maximum distance from the origin to the edge of the grid.
            float maxDist = MathF.Max(
                MathF.Max(StartPosition.Value.Length, (StartPosition.Value - drawSize).Length),
                MathF.Max((StartPosition.Value - new Vector2(drawSize.X, 0)).Length, (StartPosition.Value - new Vector2(0, drawSize.Y)).Length)
            );

            generateCircles((int)(maxDist / Spacing.Value) + 1);

            GenerateOutline(drawSize);
        }

        private void generateCircles(int count)
        {
            // Make lines the same width independent of display resolution.
            float lineWidth = 2 * DrawWidth / ScreenSpaceDrawQuad.Width;

            List<CircularContainer> generatedCircles = new List<CircularContainer>();

            for (int i = 0; i < count; i++)
            {
                // Add a minimum diameter so the center circle is clearly visible.
                float diameter = MathF.Max(lineWidth * 1.5f, i * Spacing.Value * 2);

                var gridCircle = new CircularContainer
                {
                    BorderColour = Colour4.White,
                    BorderThickness = lineWidth,
                    Alpha = 0.2f,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.None,
                    Width = diameter,
                    Height = diameter,
                    Position = StartPosition.Value,
                    Masking = true,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        AlwaysPresent = true,
                        Alpha = 0f,
                    }
                };

                generatedCircles.Add(gridCircle);
            }

            if (generatedCircles.Count == 0)
                return;

            generatedCircles.First().Alpha = 0.8f;

            AddInternal(new Container
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Children = generatedCircles,
            });
        }

        public override Vector2 GetSnappedPosition(Vector2 original)
        {
            Vector2 relativeToStart = original - StartPosition.Value;

            if (relativeToStart.LengthSquared < Precision.FLOAT_EPSILON)
                return StartPosition.Value;

            float length = relativeToStart.Length;
            float wantedLength = MathF.Round(length / Spacing.Value) * Spacing.Value;

            return StartPosition.Value + Vector2.Multiply(relativeToStart, wantedLength / length);
        }
    }
}
