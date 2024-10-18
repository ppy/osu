// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Changes;
using osuTK;

namespace osu.Game.Rulesets.Objects
{
    public static class SliderPathExtensions
    {
        /// <summary>
        /// Snaps the provided <paramref name="hitObject"/>'s duration using the <paramref name="snapProvider"/>.
        /// </summary>
        public static void SnapTo<THitObject>(this THitObject hitObject, IDistanceSnapProvider? snapProvider, NewBeatmapEditorChangeHandler? changeHandler = null)
            where THitObject : HitObject, IHasPath
        {
            double newDistance = snapProvider?.FindSnappedDistance(hitObject, (float)hitObject.Path.CalculatedDistance, DistanceSnapTarget.Start) ?? hitObject.Path.CalculatedDistance;
            new ExpectedDistanceChange(hitObject.Path, newDistance).Apply(changeHandler);
        }

        /// <summary>
        /// Reverse the direction of this path.
        /// </summary>
        /// <param name="sliderPath">The <see cref="SliderPath"/>.</param>
        /// <param name="positionalOffset">The positional offset of the resulting path. It should be added to the start position of this path.</param>
        /// <param name="changeHandler">Change handler to submit changes to.</param>
        public static void Reverse(this SliderPath sliderPath, out Vector2 positionalOffset, NewBeatmapEditorChangeHandler? changeHandler = null)
        {
            var controlPoints = sliderPath.ControlPoints;

            var inheritedLinearPoints = controlPoints.Where(p => sliderPath.PointsInSegment(p)[0].Type == PathType.LINEAR && p.Type == null).ToList();

            // Inherited points after a linear point, as well as the first control point if it inherited,
            // should be treated as linear points, so their types are temporarily changed to linear.
            inheritedLinearPoints.ForEach(p => new PathControlPointTypeChange(p, PathType.LINEAR).Apply(changeHandler));

            double[] segmentEnds = sliderPath.GetSegmentEnds().ToArray();

            // Remove segments after the end of the slider.
            for (int numSegmentsToRemove = segmentEnds.Count(se => se >= 1) - 1; numSegmentsToRemove > 0 && controlPoints.Count > 0;)
            {
                if (controlPoints.Last().Type is not null)
                {
                    numSegmentsToRemove--;
                    segmentEnds = segmentEnds[..^1];
                }

                new RemovePathControlPointChange(controlPoints, controlPoints.Count - 1).Apply(changeHandler);
            }

            // Restore original control point types.
            inheritedLinearPoints.ForEach(p => new PathControlPointTypeChange(p, null).Apply(changeHandler));

            // Recalculate middle perfect curve control points at the end of the slider path.
            if (controlPoints.Count >= 3 && controlPoints[^3].Type == PathType.PERFECT_CURVE && controlPoints[^2].Type == null && segmentEnds.Any())
            {
                double lastSegmentStart = segmentEnds.Length > 1 ? segmentEnds[^2] : 0;
                double lastSegmentEnd = segmentEnds[^1];

                var circleArcPath = new List<Vector2>();
                sliderPath.GetPathToProgress(circleArcPath, lastSegmentStart / lastSegmentEnd, 1);

                new PathControlPointPositionChange(controlPoints[^2], circleArcPath[circleArcPath.Count / 2]).Apply(changeHandler);
            }

            sliderPath.reverseControlPoints(out positionalOffset, changeHandler);
        }

        /// <summary>
        /// Reverses the order of the provided <see cref="SliderPath"/>'s <see cref="PathControlPoint"/>s.
        /// </summary>
        /// <param name="sliderPath">The <see cref="SliderPath"/>.</param>
        /// <param name="positionalOffset">The positional offset of the resulting path. It should be added to the start position of this path.</param>
        /// <param name="changeHandler">Change handler to submit changes to.</param>
        private static void reverseControlPoints(this SliderPath sliderPath, out Vector2 positionalOffset, NewBeatmapEditorChangeHandler? changeHandler = null)
        {
            var points = sliderPath.ControlPoints.ToArray();
            positionalOffset = sliderPath.PositionAt(1);

            sliderPath.ControlPoints.SubmitRemoveRange(0, sliderPath.ControlPoints.Count, changeHandler);

            PathType? lastType = null;

            for (int i = 0; i < points.Length; i++)
            {
                var p = new PathControlPoint(points[i].Position, points[i].Type);
                p.Position -= positionalOffset;

                // propagate types forwards to last null type
                if (i == points.Length - 1)
                {
                    p.Type = lastType;
                    p.Position = Vector2.Zero;
                }
                else if (p.Type != null)
                    (p.Type, lastType) = (lastType, p.Type);

                new InsertPathControlPointChange(sliderPath.ControlPoints, 0, p).Apply(changeHandler);
            }
        }

        /// <summary>
        /// Removes a range of <see cref="PathControlPoint"/>s from the provided <see cref="BindableList{T}"/>.
        /// </summary>
        public static void SubmitRemoveRange(this BindableList<PathControlPoint> controlPoints, int startIndex, int count, NewBeatmapEditorChangeHandler? changeHandler)
        {
            for (int i = 0; i < count; i++)
                new RemovePathControlPointChange(controlPoints, startIndex).Apply(changeHandler);
        }

        /// <summary>
        /// Adds a range of <see cref="PathControlPoint"/>s to the provided <see cref="BindableList{T}"/>.
        /// </summary>
        public static void SubmitAddRange(this BindableList<PathControlPoint> controlPoints, IEnumerable<PathControlPoint> points, NewBeatmapEditorChangeHandler? changeHandler)
        {
            foreach (var point in points)
                new InsertPathControlPointChange(controlPoints, controlPoints.Count, point).Apply(changeHandler);
        }
    }
}
