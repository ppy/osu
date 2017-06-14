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

        /// <summary>
        /// A list of floats decides position of each line node.
        /// </summary>
        public IEnumerable<float> Values
        {
            set
            {
                path?.Expire();
                Path localPath = new Path { RelativeSizeAxes = Axes.Both }; //capture a copy to avoid potential change
                Add(path = localPath);

                var values = value.ToArray();
                int count = Math.Max(values.Length, DefaultValueCount);

                float max = values.Max(), min = values.Min();
                if (MaxValue > max) max = MaxValue.Value;
                if (MinValue < min) min = MinValue.Value;

                for (int i = 0; i < values.Length; i++)
                {
                    float x = (i + count - values.Length) / (float)(count - 1);
                    float y = (max - values[i]) / (max - min);
                    Scheduler.AddDelayed(() => localPath.AddVertex(new Vector2(x, y)), x * transform_duration);
                }
            }
        }
    }
}
