// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public partial class SliderPlacementBlueprint : HitObjectPlacementBlueprint
    {
        public new Slider HitObject => (Slider)base.HitObject;

        [Resolved]
        private OsuHitObjectComposer? composer { get; set; }

        private SliderBodyPiece bodyPiece = null!;
        private HitCirclePiece headCirclePiece = null!;
        private HitCirclePiece tailCirclePiece = null!;
        private PathControlPointVisualiser<Slider> controlPointVisualiser = null!;

        private InputManager inputManager = null!;

        private PathControlPoint? cursor;

        private SliderPlacementState state;
        private PathControlPoint segmentStart;

        private int currentSegmentLength;
        private bool usingCustomSegmentType;

        [Resolved]
        private IDistanceSnapProvider? distanceSnapProvider { get; set; }

        [Resolved]
        private FreehandSliderToolboxGroup? freehandToolboxGroup { get; set; }

        [Resolved]
        private EditorClock? editorClock { get; set; }

        private Bindable<bool> limitedDistanceSnap { get; set; } = null!;

        private readonly IncrementalBSplineBuilder bSplineBuilder = new IncrementalBSplineBuilder { Degree = 4 };

        protected override bool IsValidForPlacement => HitObject.Path.HasValidLengthForPlacement;

        public SliderPlacementBlueprint()
            : base(new Slider())
        {
            RelativeSizeAxes = Axes.Both;

            HitObject.Path.ControlPoints.Add(segmentStart = new PathControlPoint(Vector2.Zero, PathType.LINEAR));
            currentSegmentLength = 1;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            InternalChildren = new Drawable[]
            {
                bodyPiece = new SliderBodyPiece(),
                headCirclePiece = new HitCirclePiece(),
                tailCirclePiece = new HitCirclePiece(),
                controlPointVisualiser = new PathControlPointVisualiser<Slider>(HitObject, false)
            };

            state = SliderPlacementState.Initial;
            limitedDistanceSnap = config.GetBindable<bool>(OsuSetting.EditorLimitedDistanceSnap);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager()!;

            if (freehandToolboxGroup != null)
            {
                freehandToolboxGroup.Tolerance.BindValueChanged(e =>
                {
                    bSplineBuilder.Tolerance = e.NewValue;
                    Scheduler.AddOnce(updateSliderPathFromBSplineBuilder);
                }, true);

                freehandToolboxGroup.CornerThreshold.BindValueChanged(e =>
                {
                    bSplineBuilder.CornerThreshold = e.NewValue;
                    Scheduler.AddOnce(updateSliderPathFromBSplineBuilder);
                }, true);

                freehandToolboxGroup.CircleThreshold.BindValueChanged(e =>
                {
                    Scheduler.AddOnce(updateSliderPathFromBSplineBuilder);
                }, true);
            }
        }

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public override SnapResult UpdateTimeAndPosition(Vector2 screenSpacePosition, double fallbackTime)
        {
            var result = composer?.TrySnapToNearbyObjects(screenSpacePosition, fallbackTime);
            result ??= composer?.TrySnapToDistanceGrid(screenSpacePosition, limitedDistanceSnap.Value && editorClock != null ? editorClock.CurrentTime : null);
            if (composer?.TrySnapToPositionGrid(result?.ScreenSpacePosition ?? screenSpacePosition, result?.Time ?? fallbackTime) is SnapResult gridSnapResult)
                result = gridSnapResult;
            result ??= new SnapResult(screenSpacePosition, fallbackTime);

            base.UpdateTimeAndPosition(result.ScreenSpacePosition, result.Time ?? fallbackTime);

            switch (state)
            {
                case SliderPlacementState.Initial:
                    BeginPlacement();

                    double? nearestSliderVelocity = (editorBeatmap
                                                     .HitObjects
                                                     .LastOrDefault(h => h is Slider && h.GetEndTime() < HitObject.StartTime) as Slider)?.SliderVelocityMultiplier;

                    HitObject.SliderVelocityMultiplier = nearestSliderVelocity ?? 1;
                    HitObject.Position = ToLocalSpace(result.ScreenSpacePosition);

                    // Replacing the DifficultyControlPoint above doesn't trigger any kind of invalidation.
                    // Without re-applying defaults, velocity won't be updated.
                    ApplyDefaultsToHitObject();
                    break;

                case SliderPlacementState.ControlPoints:
                    updateCursor();
                    break;
            }

            return result;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return base.OnMouseDown(e);

            switch (state)
            {
                case SliderPlacementState.Initial:
                    beginCurve();
                    break;

                case SliderPlacementState.ControlPoints:
                    if (canPlaceNewControlPoint(out var lastPoint))
                        placeNewControlPoint();
                    else if (lastPoint != null)
                        beginNewSegment(lastPoint);

                    break;
            }

            return true;
        }

        // this allows sliders to be drawn outside compose area (after starting from a point within the compose area).
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => base.ReceivePositionalInputAt(screenSpacePos) || PlacementActive == PlacementState.Active;

        // ReceivePositionalInputAtSubTree generally always returns true when masking is disabled, but we don't want that,
        // otherwise a slider path tooltip will be displayed anywhere in the editor (outside compose area).
        protected override bool ReceivePositionalInputAtSubTree(Vector2 screenSpacePos) => ReceivePositionalInputAt(screenSpacePos);

        private void beginNewSegment(PathControlPoint lastPoint)
        {
            segmentStart = lastPoint;
            segmentStart.Type = PathType.LINEAR;

            currentSegmentLength = 1;
            usingCustomSegmentType = false;
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button != MouseButton.Left)
                return base.OnDragStart(e);

            if (state != SliderPlacementState.ControlPoints)
                return base.OnDragStart(e);

            // Only enter drawing mode if no additional control points have been placed.
            int controlPointCount = HitObject.Path.ControlPoints.Count;
            if (controlPointCount > 2 || (controlPointCount == 2 && HitObject.Path.ControlPoints.Last() != cursor))
                return base.OnDragStart(e);

            bSplineBuilder.AddLinearPoint(Vector2.Zero);
            bSplineBuilder.AddLinearPoint(ToLocalSpace(e.ScreenSpaceMouseDownPosition) - HitObject.Position);
            state = SliderPlacementState.Drawing;
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            if (state == SliderPlacementState.Drawing)
            {
                bSplineBuilder.AddLinearPoint(ToLocalSpace(e.ScreenSpaceMousePosition) - HitObject.Position);
                Scheduler.AddOnce(updateSliderPathFromBSplineBuilder);
            }
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);

            if (state == SliderPlacementState.Drawing)
            {
                bSplineBuilder.Finish();
                updateSliderPathFromBSplineBuilder();

                // Change the state so it will snap the expected distance in endCurve.
                state = SliderPlacementState.Finishing;
                endCurve();
            }
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (state == SliderPlacementState.ControlPoints && e.Button == MouseButton.Right)
                endCurve();
            base.OnMouseUp(e);
        }

        private static readonly PathType[] path_types =
        [
            PathType.LINEAR,
            PathType.BEZIER,
            PathType.PERFECT_CURVE,
            PathType.BSpline(4),
        ];

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat)
                return false;

            if (state != SliderPlacementState.ControlPoints)
                return false;

            switch (e.Key)
            {
                case Key.S:
                {
                    if (!canPlaceNewControlPoint(out _))
                        return false;

                    placeNewControlPoint();
                    var last = HitObject.Path.ControlPoints.Last(p => p != cursor);
                    beginNewSegment(last);
                    return true;
                }

                case Key.Number1:
                case Key.Number2:
                case Key.Number3:
                case Key.Number4:
                {
                    if (!e.AltPressed)
                        return false;

                    usingCustomSegmentType = true;
                    segmentStart.Type = path_types[e.Key - Key.Number1];
                    controlPointVisualiser.EnsureValidPathTypes();
                    return true;
                }

                case Key.Tab:
                {
                    usingCustomSegmentType = true;

                    int currentTypeIndex = segmentStart.Type.HasValue ? Array.IndexOf(path_types, segmentStart.Type.Value) : -1;

                    if (currentTypeIndex < 0 && e.ShiftPressed)
                        currentTypeIndex = 0;

                    do
                    {
                        currentTypeIndex = (path_types.Length + currentTypeIndex + (e.ShiftPressed ? -1 : 1)) % path_types.Length;
                        segmentStart.Type = path_types[currentTypeIndex];
                        controlPointVisualiser.EnsureValidPathTypes();
                    } while (segmentStart.Type != path_types[currentTypeIndex]);

                    return true;
                }
            }

            return false;
        }

        protected override void Update()
        {
            base.Update();
            updateSlider();

            // Maintain the path type in case it got defaulted to bezier at some point during the drag.
            updatePathType();
        }

        private void beginCurve()
        {
            BeginPlacement(commitStart: true);
            state = SliderPlacementState.ControlPoints;
        }

        private void endCurve()
        {
            updateSlider();
            EndPlacement(true);
        }

        private void updatePathType()
        {
            if (usingCustomSegmentType)
            {
                controlPointVisualiser.EnsureValidPathTypes();
                return;
            }

            if (state == SliderPlacementState.Drawing)
            {
                segmentStart.Type = PathType.BSpline(4);
                return;
            }

            switch (currentSegmentLength)
            {
                case 1:
                case 2:
                    segmentStart.Type = PathType.LINEAR;
                    break;

                case 3:
                    segmentStart.Type = PathType.PERFECT_CURVE;
                    break;

                default:
                    segmentStart.Type = PathType.BEZIER;
                    break;
            }

            controlPointVisualiser.EnsureValidPathTypes();
        }

        private void updateCursor()
        {
            if (canPlaceNewControlPoint(out _))
            {
                // The cursor does not overlap a previous control point, so it can be added if not already existing.
                if (cursor == null)
                {
                    HitObject.Path.ControlPoints.Add(cursor = new PathControlPoint { Position = Vector2.Zero });

                    // The path type should be adjusted in the progression of updatePathType() (LINEAR -> PC -> BEZIER).
                    currentSegmentLength++;
                    updatePathType();
                }

                // Update the cursor position.
                cursor.Position = getCursorPosition();
            }
            else if (cursor != null)
            {
                // The cursor overlaps a previous control point, so it's removed.
                HitObject.Path.ControlPoints.Remove(cursor);
                cursor = null;

                // The path type should be adjusted in the reverse progression of updatePathType() (BEZIER -> PC -> LINEAR).
                currentSegmentLength--;
                updatePathType();
            }
        }

        private Vector2 getCursorPosition()
        {
            SnapResult? result = null;
            var mousePosition = inputManager.CurrentState.Mouse.Position;

            if (state != SliderPlacementState.ControlPoints)
            {
                result ??= composer?.TrySnapToNearbyObjects(mousePosition);
                result ??= composer?.TrySnapToDistanceGrid(mousePosition);
            }

            result ??= composer?.TrySnapToPositionGrid(mousePosition);

            return ToLocalSpace(result?.ScreenSpacePosition ?? inputManager.CurrentState.Mouse.Position) - HitObject.Position;
        }

        /// <summary>
        /// Whether a new control point can be placed at the current mouse position.
        /// </summary>
        /// <param name="lastPoint">The last-placed control point. May be null, but is not null if <c>false</c> is returned.</param>
        /// <returns>Whether a new control point can be placed at the current position.</returns>
        private bool canPlaceNewControlPoint(out PathControlPoint? lastPoint)
        {
            // We cannot rely on the ordering of drawable pieces, so find the respective drawable piece by searching for the last non-cursor control point.
            var last = HitObject.Path.ControlPoints.LastOrDefault(p => p != cursor);
            var lastPiece = controlPointVisualiser.Pieces.Single(p => p.ControlPoint == last);

            lastPoint = last;
            // We may only place a new control point if the cursor is not overlapping with the last control point.
            // If snapping is enabled, the cursor may not hover the last piece while still placing the control point at the same position.
            return !lastPiece.IsHovered && (last is null || Vector2.DistanceSquared(last.Position, getCursorPosition()) > 1f);
        }

        private void placeNewControlPoint()
        {
            // Place a new point by detatching the current cursor.
            updateCursor();
            cursor = null;
        }

        private void updateSlider()
        {
            if (state == SliderPlacementState.Drawing)
                HitObject.Path.ExpectedDistance.Value = (float)HitObject.Path.CalculatedDistance;
            else
                HitObject.Path.ExpectedDistance.Value = distanceSnapProvider?.FindSnappedDistance((float)HitObject.Path.CalculatedDistance, HitObject.StartTime, HitObject) ?? (float)HitObject.Path.CalculatedDistance;

            bodyPiece.UpdateFrom(HitObject);
            headCirclePiece.UpdateFrom(HitObject.HeadCircle);
            tailCirclePiece.UpdateFrom(HitObject.TailCircle);
        }

        private void updateSliderPathFromBSplineBuilder()
        {
            IReadOnlyList<List<Vector2>> builderPoints = bSplineBuilder.ControlPoints;

            if (builderPoints.Count == 0 || builderPoints[0].Count == 0)
                return;

            HitObject.Path.ControlPoints.Clear();

            // Iterate through generated segments and adding non-inheriting path types where appropriate.
            for (int i = 0; i < builderPoints.Count; i++)
            {
                bool isLastSegment = i == builderPoints.Count - 1;
                var segment = builderPoints[i];

                if (segment.Count == 0)
                    continue;

                // Replace this segment with a circular arc if it is a reasonable substitute.
                var circleArcSegment = tryCircleArc(segment);

                if (circleArcSegment != null)
                {
                    HitObject.Path.ControlPoints.Add(new PathControlPoint(circleArcSegment[0], PathType.PERFECT_CURVE));
                    HitObject.Path.ControlPoints.Add(new PathControlPoint(circleArcSegment[1]));
                }
                else
                {
                    HitObject.Path.ControlPoints.Add(new PathControlPoint(segment[0], PathType.BSpline(4)));
                    for (int j = 1; j < segment.Count - 1; j++)
                        HitObject.Path.ControlPoints.Add(new PathControlPoint(segment[j]));
                }

                if (isLastSegment)
                    HitObject.Path.ControlPoints.Add(new PathControlPoint(segment[^1]));
            }
        }

        private Vector2[]? tryCircleArc(List<Vector2> segment)
        {
            if (segment.Count < 3 || freehandToolboxGroup?.CircleThreshold.Value == 0) return null;

            // Assume the segment creates a reasonable circular arc and then check if it reasonable
            var points = PathApproximator.BSplineToPiecewiseLinear(segment.ToArray(), bSplineBuilder.Degree);
            var circleArcControlPoints = new[] { points[0], points[points.Count / 2], points[^1] };
            var circleArc = new CircularArcProperties(circleArcControlPoints);

            if (!circleArc.IsValid) return null;

            double length = circleArc.ThetaRange * circleArc.Radius;

            if (length > 1000) return null;

            double loss = 0;
            Vector2? lastPoint = null;
            Vector2? lastVec = null;
            Vector2? lastVec2 = null;
            int? lastDir = null;
            int? lastDir2 = null;
            double totalWinding = 0;

            // Loop through the points and check if they are not too far away from the circular arc.
            // Also make sure it curves monotonically in one direction and at most one loop is done.
            foreach (var point in points)
            {
                var vec = point - circleArc.Centre;
                loss += Math.Pow((vec.Length - circleArc.Radius) / length, 2);

                if (lastVec.HasValue)
                {
                    double det = lastVec.Value.X * vec.Y - lastVec.Value.Y * vec.X;
                    int dir = Math.Sign(det);

                    if (dir == 0)
                        continue;

                    if (lastDir.HasValue && dir != lastDir)
                        return null; // Circle center is not inside the polygon

                    lastDir = dir;
                }

                lastVec = vec;

                if (lastPoint.HasValue)
                {
                    var vec2 = point - lastPoint.Value;

                    if (lastVec2.HasValue)
                    {
                        double dot = Vector2.Dot(vec2, lastVec2.Value);
                        double det = lastVec2.Value.X * vec2.Y - lastVec2.Value.Y * vec2.X;
                        double angle = Math.Atan2(det, dot);
                        int dir2 = Math.Sign(angle);

                        if (dir2 == 0)
                            continue;

                        if (lastDir2.HasValue && dir2 != lastDir2)
                            return null; // Curvature changed, like in an S-shape

                        totalWinding += Math.Abs(angle);
                        lastDir2 = dir2;
                    }

                    lastVec2 = vec2;
                }

                lastPoint = point;
            }

            loss /= points.Count;

            return loss > freehandToolboxGroup?.CircleThreshold.Value || totalWinding > MathHelper.TwoPi ? null : circleArcControlPoints;
        }

        private enum SliderPlacementState
        {
            Initial,
            ControlPoints,
            Drawing,
            Finishing
        }
    }
}
