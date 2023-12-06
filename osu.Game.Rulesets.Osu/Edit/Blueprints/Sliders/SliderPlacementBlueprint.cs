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

        private readonly IncrementalBSplineBuilder bSplineBuilder = new IncrementalBSplineBuilder { Degree = 4 };

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

            state = SliderPlacementState.Initial;
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
            if (state == SliderPlacementState.Drawing)
                HitObject.Path.ExpectedDistance.Value = (float)HitObject.Path.CalculatedDistance;
            else
                HitObject.Path.ExpectedDistance.Value = distanceSnapProvider?.FindSnappedDistance(HitObject, (float)HitObject.Path.CalculatedDistance) ?? (float)HitObject.Path.CalculatedDistance;

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

                HitObject.Path.ControlPoints.Add(new PathControlPoint(segment[0], PathType.BSpline(4)));
                for (int j = 1; j < segment.Count - 1; j++)
                    HitObject.Path.ControlPoints.Add(new PathControlPoint(segment[j]));

                if (isLastSegment)
                    HitObject.Path.ControlPoints.Add(new PathControlPoint(segment[^1]));
            }
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
