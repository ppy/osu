// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
            double[] segmentEnds = sliderPath.GetSegmentEnds().ToArray();
            double[] distinctSegmentEnds = segmentEnds.Distinct().ToArray();

            // Remove control points at the end which do not affect the visual slider path ("invisible" control points).
            if (segmentEnds[^1] == segmentEnds[^2] && distinctSegmentEnds.Length > 1)
            {
                int numVisibleSegments = distinctSegmentEnds.Length - 2;
                var nonInheritedControlPoints = controlPoints.Where(p => p.Type is not null).ToList();

                var lastVisibleControlPoint = nonInheritedControlPoints[numVisibleSegments];
                int lastVisibleControlPointIndex = controlPoints.IndexOf(lastVisibleControlPoint);

                if (controlPoints.Count > lastVisibleControlPointIndex + 1)
                {
                    // Make sure to include all inherited control points directly after the last visible non-inherited control point.
                    do
                    {
                        lastVisibleControlPointIndex++;
                    } while (lastVisibleControlPointIndex + 1 < controlPoints.Count && controlPoints[lastVisibleControlPointIndex].Type is null);
                }

                // Remove all control points after the first invisible non-inherited control point.
                controlPoints.RemoveRange(lastVisibleControlPointIndex + 1, controlPoints.Count - lastVisibleControlPointIndex - 1);
            }

            // Recalculate perfect curve at the end of the slider path.
            if (controlPoints.Count >= 3 && controlPoints[^3].Type == PathType.PerfectCurve && controlPoints[^2].Type is null && distinctSegmentEnds.Length > 1)
            {
                double lastSegmentStart = distinctSegmentEnds[^2];
                double lastSegmentEnd = distinctSegmentEnds[^1];

                var oldCircleArcPath = new List<Vector2>();
                sliderPath.GetPathToProgress(oldCircleArcPath, lastSegmentStart / lastSegmentEnd, 1);

                var newCircleArcPoints = new[]
                {
                    oldCircleArcPath[0],
                    oldCircleArcPath[oldCircleArcPath.Count / 2],
                    oldCircleArcPath[^1]
                };

                var newCircleArcPath = PathApproximator.ApproximateCircularArc(newCircleArcPoints.AsSpan());
                controlPoints[^2].Position = newCircleArcPath[newCircleArcPath.Count / 2];
            }

            // Reverse the control points.

            var points = controlPoints.ToArray();
            positionalOffset = sliderPath.PositionAt(1);

            controlPoints.Clear();

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

                controlPoints.Insert(0, p);
            }
        }
    }
}
