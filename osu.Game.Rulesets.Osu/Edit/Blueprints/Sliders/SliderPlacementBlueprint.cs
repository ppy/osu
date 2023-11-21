// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
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
    public partial class SliderPlacementBlueprint : PlacementBlueprint
    {
        public new Slider HitObject => (Slider)base.HitObject;

        private SliderBodyPiece bodyPiece;
        private HitCirclePiece headCirclePiece;
        private HitCirclePiece tailCirclePiece;
        private PathControlPointVisualiser<Slider> controlPointVisualiser;

        private InputManager inputManager;

        private SliderPlacementState state;
        private PathControlPoint segmentStart;
        private PathControlPoint cursor;
        private int currentSegmentLength;

        [Resolved(CanBeNull = true)]
        private IPositionSnapProvider positionSnapProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider distanceSnapProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private FreehandSliderToolboxGroup freehandToolboxGroup { get; set; }

        private readonly IncrementalBSplineBuilder bSplineBuilder = new IncrementalBSplineBuilder();

        protected override bool IsValidForPlacement => HitObject.Path.HasValidLength;

        public SliderPlacementBlueprint()
            : base(new Slider())
        {
            RelativeSizeAxes = Axes.Both;

            HitObject.Path.ControlPoints.Add(segmentStart = new PathControlPoint(Vector2.Zero, PathType.LINEAR));
            currentSegmentLength = 1;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                bodyPiece = new SliderBodyPiece(),
                headCirclePiece = new HitCirclePiece(),
                tailCirclePiece = new HitCirclePiece(),
                controlPointVisualiser = new PathControlPointVisualiser<Slider>(HitObject, false)
            };

            setState(SliderPlacementState.Initial);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();

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
            }
        }

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; }

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

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
                    {
                        // Place a new point by detatching the current cursor.
                        updateCursor();
                        cursor = null;
                    }
                    else
                    {
                        // Transform the last point into a new segment.
                        Debug.Assert(lastPoint != null);

                        segmentStart = lastPoint;
                        segmentStart.Type = PathType.LINEAR;

                        currentSegmentLength = 1;
                    }

                    break;
            }

            return true;
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                switch (state)
                {
                    case SliderPlacementState.Initial:
                        return true;

                    case SliderPlacementState.ControlPoints:
                        if (HitObject.Path.ControlPoints.Count < 3)
                        {
                            var lastCp = HitObject.Path.ControlPoints.LastOrDefault();
                            if (lastCp != cursor && HitObject.Path.ControlPoints.Count == 2)
                                return false;

                            bSplineBuilder.Clear();
                            bSplineBuilder.AddLinearPoint(ToLocalSpace(e.ScreenSpaceMouseDownPosition) - HitObject.Position);
                            setState(SliderPlacementState.Drawing);
                            return true;
                        }

                        return false;
                }
            }

            return base.OnDragStart(e);
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            bSplineBuilder.AddLinearPoint(ToLocalSpace(e.ScreenSpaceMousePosition) - HitObject.Position);
            Scheduler.AddOnce(updateSliderPathFromBSplineBuilder);
        }

        private void updateSliderPathFromBSplineBuilder()
        {
            IReadOnlyList<Vector2> builderPoints = bSplineBuilder.ControlPoints;

            if (builderPoints.Count == 0)
                return;

            int lastSegmentStart = 0;
            PathType? lastPathType = null;

            HitObject.Path.ControlPoints.Clear();

            // Iterate through generated points, finding each segment and adding non-inheriting path types where appropriate.
            // Importantly, the B-Spline builder returns three Vector2s at the same location when a new segment is to be started.
            for (int i = 0; i < builderPoints.Count; i++)
            {
                bool isLastPoint = i == builderPoints.Count - 1;
                bool isNewSegment = i < builderPoints.Count - 2 && builderPoints[i] == builderPoints[i + 1] && builderPoints[i] == builderPoints[i + 2];

                if (isNewSegment || isLastPoint)
                {
                    int pointsInSegment = i - lastSegmentStart;

                    // Where possible, we can use the simpler LINEAR path type.
                    PathType? pathType = pointsInSegment == 1 ? PathType.LINEAR : PathType.BSpline(3);

                    // Linear segments can be combined, as two adjacent linear sections are computationally the same as one with the points combined.
                    if (lastPathType == pathType && lastPathType == PathType.LINEAR)
                        pathType = null;

                    HitObject.Path.ControlPoints.Add(new PathControlPoint(builderPoints[lastSegmentStart], pathType));
                    for (int j = lastSegmentStart + 1; j < i; j++)
                        HitObject.Path.ControlPoints.Add(new PathControlPoint(builderPoints[j]));

                    if (isLastPoint)
                        HitObject.Path.ControlPoints.Add(new PathControlPoint(builderPoints[i]));

                    // Skip the redundant duplicated points (see isNewSegment above) which have been coalesced into a path type.
                    lastSegmentStart = (i += 2);
                    if (pathType != null) lastPathType = pathType;
                }
            }
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            base.OnDragEnd(e);

            if (state == SliderPlacementState.Drawing)
                endCurve();
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (state == SliderPlacementState.ControlPoints && e.Button == MouseButton.Right)
                endCurve();
            base.OnMouseUp(e);
        }

        private void beginCurve()
        {
            BeginPlacement(commitStart: true);
            setState(SliderPlacementState.ControlPoints);
        }

        private void endCurve()
        {
            updateSlider();
            EndPlacement(true);
        }

        protected override void Update()
        {
            base.Update();
            updateSlider();

            // Maintain the path type in case it got defaulted to bezier at some point during the drag.
            updatePathType();
        }

        private void updatePathType()
        {
            if (state == SliderPlacementState.Drawing)
            {
                segmentStart.Type = PathType.BSpline(3);
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
                var result = positionSnapProvider?.FindSnappedPositionAndTime(inputManager.CurrentState.Mouse.Position, state == SliderPlacementState.ControlPoints ? SnapType.GlobalGrids : SnapType.All);
                cursor.Position = ToLocalSpace(result?.ScreenSpacePosition ?? inputManager.CurrentState.Mouse.Position) - HitObject.Position;
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

        /// <summary>
        /// Whether a new control point can be placed at the current mouse position.
        /// </summary>
        /// <param name="lastPoint">The last-placed control point. May be null, but is not null if <c>false</c> is returned.</param>
        /// <returns>Whether a new control point can be placed at the current position.</returns>
        private bool canPlaceNewControlPoint([CanBeNull] out PathControlPoint lastPoint)
        {
            // We cannot rely on the ordering of drawable pieces, so find the respective drawable piece by searching for the last non-cursor control point.
            var last = HitObject.Path.ControlPoints.LastOrDefault(p => p != cursor);
            var lastPiece = controlPointVisualiser.Pieces.Single(p => p.ControlPoint == last);

            lastPoint = last;
            return lastPiece.IsHovered != true;
        }

        private void updateSlider()
        {
            HitObject.Path.ExpectedDistance.Value = distanceSnapProvider?.FindSnappedDistance(HitObject, (float)HitObject.Path.CalculatedDistance) ?? (float)HitObject.Path.CalculatedDistance;

            bodyPiece.UpdateFrom(HitObject);
            headCirclePiece.UpdateFrom(HitObject.HeadCircle);
            tailCirclePiece.UpdateFrom(HitObject.TailCircle);
        }

        private void setState(SliderPlacementState newState)
        {
            state = newState;
        }

        private enum SliderPlacementState
        {
            Initial,
            ControlPoints,
            Drawing
        }
    }
}
