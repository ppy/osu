// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Utils;
using osuTK.Graphics;

namespace osu.Game.Utils
{
    public static class ColourUtils
    {
        /// <summary>
        /// Samples from a given linear gradient at a certain specified point.
        /// </summary>
        /// <param name="gradient">The gradient, defining the colour stops and their positions (in [0-1] range) in the gradient.</param>
        /// <param name="point">The point to sample the colour at.</param>
        /// <returns>A <see cref="Color4"/> sampled from the linear gradient.</returns>
        public static Color4 SampleFromLinearGradient(IReadOnlyList<(float position, Color4 colour)> gradient, float point)
        {
            if (point < gradient[0].position)
                return gradient[0].colour;

            for (int i = 0; i < gradient.Count - 1; i++)
            {
                var startStop = gradient[i];
                var endStop = gradient[i + 1];

                if (point >= endStop.position)
                    continue;

                return Interpolation.ValueAt(point, startStop.colour, endStop.colour, startStop.position, endStop.position);
            }

            return gradient[^1].colour;
        }
    }
}
