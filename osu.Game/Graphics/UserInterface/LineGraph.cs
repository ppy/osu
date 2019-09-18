// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Caching;
using osuTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osuTK.Graphics;

namespace osu.Game.Graphics.UserInterface
{
    public class LineGraph : Container
    {
        /// <summary>
        /// Manually set the max value, otherwise <see cref="Enumerable.Max(IEnumerable{float})"/> will be used.
        /// </summary>
        public float? MaxValue { get; set; }

        /// <summary>
        /// Manually set the min value, otherwise <see cref="Enumerable.Min(IEnumerable{float})"/> will be used.
        /// </summary>
        public float? MinValue { get; set; }

        public float ActualMaxValue { get; private set; } = float.NaN;
        public float ActualMinValue { get; private set; } = float.NaN;

        private const double transform_duration = 1500;

        /// <summary>
        /// Hold an empty area if values are less.
        /// </summary>
        public int DefaultValueCount;

        private readonly Container<Path> maskingContainer;
        private readonly Path path;

        private float[] values;

        /// <summary>
        /// A list of floats decides position of each line node.
        /// </summary>
        public IEnumerable<float> Values
        {
            get => values;
            set
            {
                values = value.ToArray();

                float max = values.Max(), min = values.Min();
                if (MaxValue > max) max = MaxValue.Value;
                if (MinValue < min) min = MinValue.Value;

                ActualMaxValue = max;
                ActualMinValue = min;

                pathCached.Invalidate();

                maskingContainer.Width = 0;
                maskingContainer.ResizeWidthTo(1, transform_duration, Easing.OutQuint);
            }
        }

        public Color4 LineColour
        {
            get => maskingContainer.Colour;
            set => maskingContainer.Colour = value;
        }

        public LineGraph()
        {
            Add(maskingContainer = new Container<Path>
            {
                Masking = true,
                RelativeSizeAxes = Axes.Both,
                Child = path = new SmoothPath
                {
                    AutoSizeAxes = Axes.None,
                    RelativeSizeAxes = Axes.Both,
                    PathRadius = 1
                }
            });
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.DrawSize) > 0)
                pathCached.Invalidate();

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        private readonly Cached pathCached = new Cached();

        protected override void Update()
        {
            base.Update();

            if (!pathCached.IsValid)
            {
                applyPath();
                pathCached.Validate();
            }
        }

        private void applyPath()
        {
            path.ClearVertices();
            if (values == null) return;

            int count = Math.Max(values.Length, DefaultValueCount);

            for (int i = 0; i < values.Length; i++)
            {
                // Make sure that we are accounting for path width when calculating vertex positions
                // We need to apply 2x the path radius to account for it because the full diameter of the line accounts into height
                float x = (i + count - values.Length) / (float)(count - 1) * (DrawWidth - 2 * path.PathRadius);
                float y = GetYPosition(values[i]) * (DrawHeight - 2 * path.PathRadius);
                path.AddVertex(new Vector2(x, y));
            }
        }

        protected float GetYPosition(float value)
        {
            if (ActualMaxValue == ActualMinValue) return 0;

            return (ActualMaxValue - value) / (ActualMaxValue - ActualMinValue);
        }
    }
}
