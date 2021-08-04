// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

#nullable enable

namespace osu.Game.Rulesets.Objects
{
    public static class SliderPathExtensions
    {
        /// <summary>
        /// Reverse the direction of this path.
        /// </summary>
        /// <param name="sliderPath">The <see cref="SliderPath"/>.</param>
        /// <param name="positionalOffset">The positional offset of the resulting path. It should be added to the start position of this path.</param>
        public static void Reverse(this SliderPath sliderPath, out Vector2 positionalOffset)
        {
            var points = sliderPath.ControlPoints.ToArray();
            positionalOffset = points.Last().Position.Value;

            sliderPath.ControlPoints.Clear();

            PathType? lastType = null;

            for (var i = 0; i < points.Length; i++)
            {
                var p = points[i];
                p.Position.Value -= positionalOffset;

                // propagate types forwards to last null type
                if (i == points.Length - 1)
                    p.Type.Value = lastType;
                else if (p.Type.Value != null)
                {
                    var newType = p.Type.Value;
                    p.Type.Value = lastType;
                    lastType = newType;
                }

                sliderPath.ControlPoints.Insert(0, p);
            }
        }
    }
}
