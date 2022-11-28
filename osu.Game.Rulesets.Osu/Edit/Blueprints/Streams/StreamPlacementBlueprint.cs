// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Streams.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Streams
{
    public partial class StreamPlacementBlueprint : PlacementBlueprint
    {
        public new Stream HitObject => (Stream)base.HitObject;

        private StreamPiece streamPiece = null!;
        private PathControlPointVisualiser<Stream> controlPointVisualiser = null!;

        private InputManager inputManager = null!;

        private StreamPlacementState state;
        private PathControlPoint segmentStart;
        private PathControlPoint? cursor;
        private StreamControlPoint streamSegmentStart;
        private StreamControlPoint? streamCursor;
        private int currentSegmentLength;

        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider? snapProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler? changeHandler { get; set; }

        public StreamPlacementBlueprint()
            : base(new Stream())
        {
            RelativeSizeAxes = Axes.Both;

            HitObject.Path.ControlPoints.Add(segmentStart = new PathControlPoint(Vector2.Zero, PathType.Linear));
            HitObject.StreamPath.ControlPoints.Add(streamSegmentStart = new StreamControlPoint());
            currentSegmentLength = 1;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                streamPiece = new StreamPiece(),
                controlPointVisualiser = new PathControlPointVisualiser<Stream>(HitObject, false)
            };

            setState(StreamPlacementState.Initial);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        public override void UpdateTimeAndPosition(SnapResult result)
        {
            base.UpdateTimeAndPosition(result);

            switch (state)
            {
                case StreamPlacementState.Initial:
                    BeginPlacement();

                    var nearestDifficultyPoint = editorBeatmap.HitObjects
                                                              .LastOrDefault(h => h is Slider && h.GetEndTime() < HitObject.StartTime)?
                                                              .DifficultyControlPoint?.DeepClone() as DifficultyControlPoint;

                    HitObject.DifficultyControlPoint = nearestDifficultyPoint ?? new DifficultyControlPoint();
                    HitObject.Position = ToLocalSpace(result.ScreenSpacePosition);

                    // Replacing the DifficultyControlPoint above doesn't trigger any kind of invalidation.
                    // Without re-applying defaults, velocity won't be updated.
                    ApplyDefaultsToHitObject();
                    break;

                case StreamPlacementState.Body:
                    updateCursor();
                    updateStreamCursor();
                    break;
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button != MouseButton.Left)
                return base.OnMouseDown(e);

            switch (state)
            {
                case StreamPlacementState.Initial:
                    beginCurve();
                    break;

                case StreamPlacementState.Body:
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

                        if (lastPoint.Type == null)
                        {
                            updateStreamCursor();
                            streamSegmentStart = streamCursor!;
                            streamCursor = null;
                        }

                        segmentStart = lastPoint;
                        segmentStart.Type = PathType.Linear;

                        currentSegmentLength = 1;
                    }

                    break;
            }

            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (state == StreamPlacementState.Body && e.Button == MouseButton.Right)
                endCurve();
            base.OnMouseUp(e);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (!e.ShiftPressed || streamCursor == null)
                return base.OnScroll(e);

            streamCursor.Acceleration += e.ScrollDelta.X * 0.5d;

            return true;
        }

        private void beginCurve()
        {
            BeginPlacement(commitStart: true);
            setState(StreamPlacementState.Body);
        }

        private void endCurve()
        {
            updateStream();
            EndPlacement(HitObject.Path.HasValidLength);
            editorBeatmap.SelectedHitObjects.Add(HitObject);
        }

        protected override void Update()
        {
            base.Update();
            updateStream();

            // Maintain the path type in case it got defaulted to bezier at some point during the drag.
            updatePathType();
        }

        private void updatePathType()
        {
            switch (currentSegmentLength)
            {
                case 1:
                case 2:
                    segmentStart.Type = PathType.Linear;
                    break;

                case 3:
                    segmentStart.Type = PathType.PerfectCurve;
                    break;

                default:
                    segmentStart.Type = PathType.Bezier;
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

                    // The path type should be adjusted in the progression of updatePathType() (Linear -> PC -> Bezier).
                    currentSegmentLength++;
                    updatePathType();
                }

                // Update the cursor position.
                var result = snapProvider?.FindSnappedPositionAndTime(inputManager.CurrentState.Mouse.Position);
                cursor.Position = ToLocalSpace(result?.ScreenSpacePosition ?? inputManager.CurrentState.Mouse.Position) - HitObject.Position;
            }
            else if (cursor != null)
            {
                // The cursor overlaps a previous control point, so it's removed.
                HitObject.Path.ControlPoints.Remove(cursor);
                cursor = null;

                // The path type should be adjusted in the reverse progression of updatePathType() (Bezier -> PC -> Linear).
                currentSegmentLength--;
                updatePathType();
            }
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
            return lastPiece.IsHovered != true;
        }

        private void updateStreamCursor()
        {
            if (canPlaceNewControlPoint(out var lastPoint) || lastPoint!.Type == null)
            {
                // The cursor does not overlap a previous non-inherit control point, so a valid new segment can be added.
                if (streamCursor == null)
                {
                    HitObject.StreamPath.ControlPoints.Add(streamCursor = new StreamControlPoint());
                }

                double time = EditorClock.CurrentTime;
                streamCursor.Time = Math.Max(time - HitObject.StartTime, streamSegmentStart.Time + editorBeatmap.GetBeatLengthAtTime(streamSegmentStart.Time));
                streamCursor.Count = (int)Math.Round((streamCursor.Time - streamSegmentStart.Time) / editorBeatmap.GetBeatLengthAtTime(time));
            }
            else if (streamCursor != null)
            {
                // The cursor overlaps a previous non-inherit control point, so it's removed.
                HitObject.StreamPath.ControlPoints.Remove(streamCursor);
                streamCursor = null;
            }
        }

        private void updateStream()
        {
            //HitObject.Path.ExpectedDistance.Value = snapProvider?.FindSnappedDistance(HitObject, (float)HitObject.Path.CalculatedDistance) ?? (float)HitObject.Path.CalculatedDistance;
            streamPiece.UpdateFrom(HitObject);
        }

        private void setState(StreamPlacementState newState)
        {
            state = newState;
        }

        private enum StreamPlacementState
        {
            Initial,
            Body,
        }
    }
}
