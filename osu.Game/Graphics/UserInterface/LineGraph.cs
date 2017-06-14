// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;

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

        private const float transform_duration = 250;

        /// <summary>
        /// Hold an empty area if values are less.
        /// </summary>
        public int DefaultValueCount;

        private Path path;

        private float[] values;

        /// <summary>
        /// A list of floats decides position of each line node.
        /// </summary>
        public IEnumerable<float> Values
        {
            set
            {
                values = value.ToArray();
                applyPath();
            }
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & Invalidation.DrawSize) != 0)
                applyPath();
            return base.Invalidate(invalidation, source, shallPropagate);
        }

        private void applyPath()
        {
            if (values == null) return;

            path?.Expire();
            Path localPath = new Path { RelativeSizeAxes = Axes.Both, PathWidth = 1 }; //capture a copy to avoid potential change
            Add(path = localPath);

            int count = Math.Max(values.Length, DefaultValueCount);

            float max = values.Max(), min = values.Min();
            if (MaxValue > max) max = MaxValue.Value;
            if (MinValue < min) min = MinValue.Value;

            for (int i = 0; i < values.Length; i++)
            {
                float x = (i + count - values.Length) / (float)(count - 1) * DrawWidth - 1;
                float y = (max - values[i]) / (max - min) * DrawHeight - 1;
                // the -1 is for inner offset in path (actually -PathWidth)
                localPath.AddVertex(new Vector2(x, y));
            }
        }
    }
}
