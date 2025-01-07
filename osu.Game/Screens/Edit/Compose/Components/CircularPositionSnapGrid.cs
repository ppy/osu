// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
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

            // Calculate the required number of circles based on the maximum distance from the origin to the edge of the grid.
            float dx = Math.Max(StartPosition.Value.X, DrawWidth - StartPosition.Value.X);
            float dy = Math.Max(StartPosition.Value.Y, DrawHeight - StartPosition.Value.Y);
            float maxDistance = new Vector2(dx, dy).Length;
            // We need to add one because the first circle starts at zero radius.
            int requiredCircles = (int)(maxDistance / Spacing.Value) + 1;

            generateCircles(requiredCircles);
            GenerateOutline(drawSize);
        }

        private void generateCircles(int count)
        {
            // Make lines the same width independent of display resolution.
            float lineWidth = 2 * DrawWidth / ScreenSpaceDrawQuad.Width;

            List<CircularProgress> generatedCircles = new List<CircularProgress>();

            for (int i = 0; i < count; i++)
            {
                // Add a minimum diameter so the center circle is clearly visible.
                float diameter = MathF.Max(lineWidth * 1.5f, i * Spacing.Value * 2);

                var gridCircle = new CircularProgress
                {
                    Position = StartPosition.Value,
                    Origin = Anchor.Centre,
                    Size = new Vector2(diameter),
                    InnerRadius = lineWidth * 1f / diameter,
                    Colour = Colour4.White,
                    Alpha = 0.2f,
                    Progress = 1,
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
