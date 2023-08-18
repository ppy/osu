// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public static class SliderPathExtensions
    {
        /// <summary>
        /// Snaps the provided <paramref name="hitObject"/>'s duration using the <paramref name="snapProvider"/>.
        /// </summary>
        public static void SnapTo<THitObject>(this THitObject hitObject, IDistanceSnapProvider? snapProvider)
            where THitObject : HitObject, IHasPath
        {
            hitObject.Path.ExpectedDistance.Value = snapProvider?.FindSnappedDistance(hitObject, (float)hitObject.Path.CalculatedDistance) ?? hitObject.Path.CalculatedDistance;
        }

        /// <summary>
        /// Reverse the direction of this path.
        /// </summary>
        /// <param name="sliderPath">The <see cref="SliderPath"/>.</param>
        /// <param name="positionalOffset">The positional offset of the resulting path. It should be added to the start position of this path.</param>
        public static void Reverse(this SliderPath sliderPath, out Vector2 positionalOffset)
        {
            var controlPoints = sliderPath.ControlPoints;

            var inheritedLinearPoints = controlPoints.Where(p => sliderPath.PointsInSegment(p)[0].Type == PathType.Linear && p.Type is null).ToList();

            if (controlPoints[0].Type == null)
            {
                inheritedLinearPoints.Add(controlPoints[0]);
            }

            // Inherited points after a linear point, as well as the first control point if it inherited,
            // should be treated as linear points, so their types are temporarily changed to linear.
            inheritedLinearPoints.ForEach(p => p.Type = PathType.Linear);

            double[] segmentEnds = sliderPath.GetSegmentEnds().ToArray();
            double[] distinctSegmentEnds = truncateEndingDuplicates(segmentEnds);

            // Remove control points at the end which do not affect the visual slider path ("invisible" control points).
            if (segmentEnds.Length >= 2 && Precision.AlmostEquals(segmentEnds[^1], segmentEnds[^2]) && distinctSegmentEnds.Length > 0)
            {
                int numVisibleSegments = distinctSegmentEnds.Length - 1;
                var nonInheritedControlPoints = controlPoints.Where(p => p.Type is not null).ToList();

                int lastVisibleControlPointIndex = controlPoints.IndexOf(nonInheritedControlPoints[numVisibleSegments]);

                // Make sure to include all inherited control points directly after the last visible non-inherited control point.
                while (lastVisibleControlPointIndex + 1 < controlPoints.Count)
                {
                    lastVisibleControlPointIndex++;

                    if (controlPoints[lastVisibleControlPointIndex].Type is not null)
                        break;
                }

                // Remove all control points after the first invisible non-inherited control point.
                controlPoints.RemoveRange(lastVisibleControlPointIndex + 1, controlPoints.Count - lastVisibleControlPointIndex - 1);
            }

            // Restore original control point types.
            inheritedLinearPoints.ForEach(p => p.Type = null);

            // Recalculate perfect curve at the end of the slider path.
            if (controlPoints.Count >= 3 && controlPoints[^3].Type == PathType.PerfectCurve && controlPoints[^2].Type is null && distinctSegmentEnds.Length > 0)
            {
                double lastSegmentStart = distinctSegmentEnds.Length > 1 ? distinctSegmentEnds[^2] : 0;
                double lastSegmentEnd = distinctSegmentEnds[^1];

                var circleArcPath = new List<Vector2>();
                sliderPath.GetPathToProgress(circleArcPath, lastSegmentStart / lastSegmentEnd, 1);

                controlPoints[^2].Position = circleArcPath[circleArcPath.Count / 2];
            }

            sliderPath.reverseControlPoints(out positionalOffset);
        }

        /// <summary>
        /// Keeps removing the last element of the provided array until the last two elements are not equal.
        /// </summary>
        /// <param name="arr">The array to truncate.</param>
        /// <returns>The truncated array.</returns>
        private static double[] truncateEndingDuplicates(double[] arr)
        {
            if (arr.Length < 2)
                return arr;

            var result = arr.ToList();

            while (result.Count > 1 && Precision.AlmostEquals(result[^1], result[^2]))
                result.RemoveAt(result.Count - 1);

            return result.ToArray();
        }

        /// <summary>
        /// Reverses the order of the provided <see cref="SliderPath"/>'s <see cref="PathControlPoint"/>s.
        /// </summary>
        /// <param name="sliderPath">The <see cref="SliderPath"/>.</param>
        /// <param name="positionalOffset">The positional offset of the resulting path. It should be added to the start position of this path.</param>
        private static void reverseControlPoints(this SliderPath sliderPath, out Vector2 positionalOffset)
        {
            var points = sliderPath.ControlPoints.ToArray();
            positionalOffset = sliderPath.PositionAt(1);

            sliderPath.ControlPoints.Clear();

            PathType? lastType = null;

            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];
                p.Position -= positionalOffset;

                // propagate types forwards to last null type
                if (i == points.Length - 1)
                {
                    p.Type = lastType;
                    p.Position = Vector2.Zero;
                }
                else if (p.Type != null)
                    (p.Type, lastType) = (lastType, p.Type);

                sliderPath.ControlPoints.Insert(0, p);
            }
        }
    }
}
