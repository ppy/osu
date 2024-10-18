// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Screens.Edit.Changes
{
    /// <summary>
    /// Reverse the direction of this path.
    /// </summary>
    public class ReverseSliderPathChange : CompositeChange
    {
        /// <summary>
        /// The positional offset of the resulting path. It should be added to the start position of the path.
        /// </summary>
        public Vector2 PositionalOffset { get; private set; }

        private readonly SliderPath sliderPath;

        /// <summary>
        /// Reverse the direction of this path.
        /// </summary>
        /// <param name="sliderPath">The <see cref="SliderPath"/>.</param>
        public ReverseSliderPathChange(SliderPath sliderPath)
        {
            this.sliderPath = sliderPath;
        }

        protected override void SubmitChanges()
        {
            var controlPoints = sliderPath.ControlPoints;

            var inheritedLinearPoints = controlPoints.Where(p => sliderPath.PointsInSegment(p)[0].Type == PathType.LINEAR && p.Type == null).ToList();

            // Inherited points after a linear point, as well as the first control point if it inherited,
            // should be treated as linear points, so their types are temporarily changed to linear.
            inheritedLinearPoints.ForEach(p => Submit(new PathControlPointTypeChange(p, PathType.LINEAR)));

            double[] segmentEnds = sliderPath.GetSegmentEnds().ToArray();

            // Remove segments after the end of the slider.
            for (int numSegmentsToRemove = segmentEnds.Count(se => se >= 1) - 1; numSegmentsToRemove > 0 && controlPoints.Count > 0;)
            {
                if (controlPoints.Last().Type is not null)
                {
                    numSegmentsToRemove--;
                    segmentEnds = segmentEnds[..^1];
                }

                Submit(new RemovePathControlPointChange(controlPoints, controlPoints.Count - 1));
            }

            // Restore original control point types.
            inheritedLinearPoints.ForEach(p => Submit(new PathControlPointTypeChange(p, null)));

            // Recalculate middle perfect curve control points at the end of the slider path.
            if (controlPoints.Count >= 3 && controlPoints[^3].Type == PathType.PERFECT_CURVE && controlPoints[^2].Type == null && segmentEnds.Any())
            {
                double lastSegmentStart = segmentEnds.Length > 1 ? segmentEnds[^2] : 0;
                double lastSegmentEnd = segmentEnds[^1];

                var circleArcPath = new List<Vector2>();
                sliderPath.GetPathToProgress(circleArcPath, lastSegmentStart / lastSegmentEnd, 1);

                Submit(new PathControlPointPositionChange(controlPoints[^2], circleArcPath[circleArcPath.Count / 2]));
            }

            reverseControlPoints();
        }

        private void reverseControlPoints()
        {
            var points = sliderPath.ControlPoints.ToArray();
            PositionalOffset = sliderPath.PositionAt(1);

            Submit(new RemoveRangePathControlPointChange(sliderPath.ControlPoints, 0, sliderPath.ControlPoints.Count));

            PathType? lastType = null;

            for (int i = 0; i < points.Length; i++)
            {
                var p = new PathControlPoint(points[i].Position, points[i].Type);
                p.Position -= PositionalOffset;

                // propagate types forwards to last null type
                if (i == points.Length - 1)
                {
                    p.Type = lastType;
                    p.Position = Vector2.Zero;
                }
                else if (p.Type != null)
                    (p.Type, lastType) = (lastType, p.Type);

                Submit(new InsertPathControlPointChange(sliderPath.ControlPoints, 0, p));
            }
        }
    }
}
