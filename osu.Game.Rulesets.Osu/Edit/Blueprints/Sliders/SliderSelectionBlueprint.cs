// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public partial class SliderSelectionBlueprint : OsuSelectionBlueprint<Slider>
    {
        protected new DrawableSlider DrawableObject => (DrawableSlider)base.DrawableObject;

        protected SliderBodyPiece BodyPiece { get; private set; } = null!;
        protected SliderCircleOverlay HeadOverlay { get; private set; } = null!;
        protected SliderCircleOverlay TailOverlay { get; private set; } = null!;

        protected PathControlPointVisualiser<Slider>? ControlPointVisualiser { get; private set; }

        [Resolved]
        private IDistanceSnapProvider? distanceSnapProvider { get; set; }

        [Resolved]
        private IPlacementHandler? placementHandler { get; set; }

        [Resolved]
        private EditorBeatmap? editorBeatmap { get; set; }

        [Resolved]
        private IEditorChangeHandler? changeHandler { get; set; }

        [Resolved]
        private BindableBeatDivisor? beatDivisor { get; set; }

        private PathControlPoint? placementControlPoint;

        public override Quad SelectionQuad
        {
            get
            {
                var result = BodyPiece.ScreenSpaceDrawQuad.AABBFloat;

                result = RectangleF.Union(result, HeadOverlay.VisibleQuad);
                result = RectangleF.Union(result, TailOverlay.VisibleQuad);

                if (ControlPointVisualiser != null)
                {
                    foreach (var piece in ControlPointVisualiser.Pieces)
                        result = RectangleF.Union(result, piece.ScreenSpaceDrawQuad.AABBFloat);
                }

                return result;
            }
        }

        private readonly BindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly IBindable<int> pathVersion = new Bindable<int>();
        private readonly BindableList<HitObject> selectedObjects = new BindableList<HitObject>();
        private readonly Bindable<bool> showHitMarkers = new Bindable<bool>();

        // Cached slider path which ignored the expected distance value.
        private readonly Cached<SliderPath> fullPathCache = new Cached<SliderPath>();

        private Vector2 lastRightClickPosition;

        public SliderSelectionBlueprint(Slider slider)
            : base(slider)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            InternalChildren = new Drawable[]
            {
                BodyPiece = new SliderBodyPiece(),
                HeadOverlay = CreateCircleOverlay(HitObject, SliderPosition.Start),
                TailOverlay = CreateCircleOverlay(HitObject, SliderPosition.End),
            };

            // tail will always have a non-null end drag marker.
            Debug.Assert(TailOverlay.EndDragMarker != null);

            TailOverlay.EndDragMarker.StartDrag += startAdjustingLength;
            TailOverlay.EndDragMarker.Drag += adjustLength;
            TailOverlay.EndDragMarker.EndDrag += endAdjustLength;

            config.BindWith(OsuSetting.EditorShowHitMarkers, showHitMarkers);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlPoints.BindTo(HitObject.Path.ControlPoints);
            controlPoints.CollectionChanged += (_, _) => fullPathCache.Invalidate();

            pathVersion.BindTo(HitObject.Path.Version);
            pathVersion.BindValueChanged(_ => editorBeatmap?.Update(HitObject));

            BodyPiece.UpdateFrom(HitObject);

            if (editorBeatmap != null)
                selectedObjects.BindTo(editorBeatmap.SelectedHitObjects);
            selectedObjects.BindCollectionChanged((_, _) => updateVisualDefinition(), true);
            showHitMarkers.BindValueChanged(_ =>
            {
                if (!showHitMarkers.Value)
                    DrawableObject.RestoreHitAnimations();
            });
        }

        public override bool HandleQuickDeletion()
        {
            var hoveredControlPoint = ControlPointVisualiser?.Pieces.FirstOrDefault(p => p.IsHovered);

            if (hoveredControlPoint == null)
                return false;

            if (hoveredControlPoint.IsSelected.Value)
                ControlPointVisualiser?.DeleteSelected();
            else
                ControlPointVisualiser?.Delete([hoveredControlPoint.ControlPoint]);

            return true;
        }

        protected override void Update()
        {
            base.Update();

            if (IsSelected)
                BodyPiece.UpdateFrom(HitObject);

            if (showHitMarkers.Value)
                DrawableObject.SuppressHitAnimations();
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateVisualDefinition();
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateVisualDefinition();
            base.OnHoverLost(e);
        }

        protected override void OnSelected()
        {
            updateVisualDefinition();
            base.OnSelected();
        }

        protected override void OnDeselected()
        {
            base.OnDeselected();

            if (placementControlPoint != null)
                endControlPointPlacement();

            updateVisualDefinition();
            BodyPiece.RecyclePath();
        }

        private void updateVisualDefinition()
        {
            // To reduce overhead of drawing these blueprints, only add extra detail when only this slider is selected.
            if (IsSelected && selectedObjects.Count < 2)
            {
                if (ControlPointVisualiser == null)
                {
                    AddInternal(ControlPointVisualiser = new PathControlPointVisualiser<Slider>(HitObject, true)
                    {
                        RemoveControlPointsRequested = removeControlPoints,
                        SplitControlPointsRequested = splitControlPoints
                    });
                }
            }
            else
            {
                ControlPointVisualiser?.Expire();
                ControlPointVisualiser = null;
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Right:
                    lastRightClickPosition = e.MouseDownPosition;
                    return false; // Allow right click to be handled by context menu

                case MouseButton.Left:

                    // If there's more than two objects selected, ctrl+click should deselect
                    if (e.ControlPressed && IsSelected && selectedObjects.Count < 2)
                    {
                        changeHandler?.BeginChange();
                        placementControlPoint = addControlPoint(e.MousePosition);
                        ControlPointVisualiser?.SetSelectionTo(placementControlPoint);
                        return true; // Stop input from being handled and modifying the selection
                    }

                    break;
            }

            return false;
        }

        #region Length Adjustment (independent of path nodes)

        private Vector2 lengthAdjustMouseOffset;
        private double oldDuration;
        private double oldVelocityMultiplier;
        private double desiredDistance;
        private bool isAdjustingLength;
        private bool adjustVelocityMomentary;

        private void startAdjustingLength(DragStartEvent e)
        {
            isAdjustingLength = true;
            adjustVelocityMomentary = e.ShiftPressed;
            lengthAdjustMouseOffset = ToLocalSpace(e.ScreenSpaceMouseDownPosition) - HitObject.Position - HitObject.Path.PositionAt(1);
            oldDuration = HitObject.Path.Distance / HitObject.SliderVelocityMultiplier;
            oldVelocityMultiplier = HitObject.SliderVelocityMultiplier;
            changeHandler?.BeginChange();
        }

        private void endAdjustLength()
        {
            trimExcessControlPoints(HitObject.Path);
            changeHandler?.EndChange();
            isAdjustingLength = false;
        }

        private void adjustLength(MouseEvent e) => adjustLength(findClosestPathDistance(e), e.ShiftPressed);

        private void adjustLength(double proposedDistance, bool adjustVelocity)
        {
            desiredDistance = proposedDistance;
            double proposedVelocity = oldVelocityMultiplier;

            if (adjustVelocity)
            {
                proposedVelocity = proposedDistance / oldDuration;
                proposedDistance = Math.Clamp(proposedDistance, 0.1 * oldDuration, 10 * oldDuration);
            }
            else
            {
                double minDistance = distanceSnapProvider?.GetBeatSnapDistance() * oldVelocityMultiplier ?? 1;
                // Add a small amount to the proposed distance to make it easier to snap to the full length of the slider.
                proposedDistance = distanceSnapProvider?.FindSnappedDistance((float)proposedDistance + 1, HitObject.StartTime, HitObject) ?? proposedDistance;
                proposedDistance = Math.Clamp(proposedDistance, minDistance, HitObject.Path.CalculatedDistance);
            }

            if (Precision.AlmostEquals(proposedDistance, HitObject.Path.Distance) && Precision.AlmostEquals(proposedVelocity, HitObject.SliderVelocityMultiplier))
                return;

            HitObject.SliderVelocityMultiplier = proposedVelocity;
            HitObject.Path.ExpectedDistance.Value = proposedDistance;
            editorBeatmap?.Update(HitObject);
        }

        /// <summary>
        /// Trims control points from the end of the slider path which are not required to reach the expected end of the slider.
        /// </summary>
        /// <param name="sliderPath">The slider path to trim control points of.</param>
        private void trimExcessControlPoints(SliderPath sliderPath)
        {
            if (!sliderPath.ExpectedDistance.Value.HasValue)
                return;

            double[] segmentEnds = sliderPath.GetSegmentEnds().ToArray();
            int segmentIndex = 0;

            for (int i = 1; i < sliderPath.ControlPoints.Count - 1; i++)
            {
                if (!sliderPath.ControlPoints[i].Type.HasValue) continue;

                if (Precision.AlmostBigger(segmentEnds[segmentIndex], 1, 1E-3))
                {
                    sliderPath.ControlPoints.RemoveRange(i + 1, sliderPath.ControlPoints.Count - i - 1);
                    sliderPath.ControlPoints[^1].Type = null;
                    break;
                }

                segmentIndex++;
            }
        }

        /// <summary>
        /// Finds the expected distance value for which the slider end is closest to the mouse position.
        /// </summary>
        private double findClosestPathDistance(MouseEvent e)
        {
            const double step1 = 10;
            const double step2 = 0.1;
            const double longer_distance_bias = 0.01;

            var desiredPosition = ToLocalSpace(e.ScreenSpaceMousePosition) - HitObject.Position - lengthAdjustMouseOffset;

            if (!fullPathCache.IsValid)
                fullPathCache.Value = new SliderPath(HitObject.Path.ControlPoints.ToArray());

            // Do a linear search to find the closest point on the path to the mouse position.
            double bestValue = 0;
            double minDistance = double.MaxValue;

            for (double d = 0; d <= fullPathCache.Value.CalculatedDistance; d += step1)
            {
                double t = d / fullPathCache.Value.CalculatedDistance;
                double dist = Vector2.Distance(fullPathCache.Value.PositionAt(t), desiredPosition) - d * longer_distance_bias;

                if (dist >= minDistance) continue;

                minDistance = dist;
                bestValue = d;
            }

            // Do another linear search to fine-tune the result.
            double maxValue = Math.Min(bestValue + step1, fullPathCache.Value.CalculatedDistance);

            for (double d = bestValue - step1; d <= maxValue; d += step2)
            {
                double t = d / fullPathCache.Value.CalculatedDistance;
                double dist = Vector2.Distance(fullPathCache.Value.PositionAt(t), desiredPosition) - d * longer_distance_bias;

                if (dist >= minDistance) continue;

                minDistance = dist;
                bestValue = d;
            }

            return bestValue;
        }

        #endregion

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (placementControlPoint == null)
                return base.OnDragStart(e);

            ControlPointVisualiser?.DragStarted(placementControlPoint);
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            base.OnDrag(e);

            if (placementControlPoint != null)
                ControlPointVisualiser?.DragInProgress(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (placementControlPoint != null)
                endControlPointPlacement();
        }

        private void endControlPointPlacement()
        {
            if (IsDragged)
                ControlPointVisualiser?.DragEnded();

            placementControlPoint = null;
            changeHandler?.EndChange();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!IsSelected)
                return false;

            if (e.Key == Key.F && e.ControlPressed && e.ShiftPressed)
            {
                convertToStream();
                return true;
            }

            if (isAdjustingLength && e.ShiftPressed != adjustVelocityMomentary)
            {
                adjustVelocityMomentary = e.ShiftPressed;
                adjustLength(desiredDistance, adjustVelocityMomentary);
                return true;
            }

            return false;
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (!IsSelected || !isAdjustingLength || e.ShiftPressed == adjustVelocityMomentary) return;

            adjustVelocityMomentary = e.ShiftPressed;
            adjustLength(desiredDistance, adjustVelocityMomentary);
        }

        private PathControlPoint addControlPoint(Vector2 position)
        {
            position -= HitObject.Position;

            int insertionIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                float dist = new Line(controlPoints[i].Position, controlPoints[i + 1].Position).DistanceToPoint(position);

                if (dist < minDistance)
                {
                    insertionIndex = i + 1;
                    minDistance = dist;
                }
            }

            var pathControlPoint = new PathControlPoint { Position = position };

            // Move the control points from the insertion index onwards to make room for the insertion
            controlPoints.Insert(insertionIndex, pathControlPoint);

            ControlPointVisualiser?.EnsureValidPathTypes();

            HitObject.SnapTo(distanceSnapProvider);

            return pathControlPoint;
        }

        private void removeControlPoints(List<PathControlPoint> toRemove)
        {
            // Ensure that there are any points to be deleted
            if (toRemove.Count == 0)
                return;

            foreach (var c in toRemove)
            {
                // The first control point in the slider must have a type, so take it from the previous "first" one
                // Todo: Should be handled within SliderPath itself
                if (c == controlPoints[0] && controlPoints.Count > 1 && controlPoints[1].Type == null)
                    controlPoints[1].Type = controlPoints[0].Type;

                controlPoints.Remove(c);
            }

            ControlPointVisualiser?.EnsureValidPathTypes();

            // Snap the slider to the current beat divisor before checking length validity.
            HitObject.SnapTo(distanceSnapProvider);

            // If there are 0 or 1 remaining control points, or the slider has an invalid length, it is in a degenerate form and should be deleted
            if (controlPoints.Count <= 1 || !HitObject.Path.HasValidLengthForPlacement)
            {
                placementHandler?.Delete(HitObject);
                return;
            }

            // The path will have a non-zero offset if the head is removed, but sliders don't support this behaviour since the head is positioned at the slider's position
            // So the slider needs to be offset by this amount instead, and all control points offset backwards such that the path is re-positioned at (0, 0)
            Vector2 first = controlPoints[0].Position;
            foreach (var c in controlPoints)
                c.Position -= first;
            HitObject.Position += first;
        }

        private void splitControlPoints(List<PathControlPoint> controlPointsToSplitAt)
        {
            if (editorBeatmap == null)
                return;

            // Arbitrary gap in milliseconds to put between split slider pieces
            const double split_gap = 100;

            // Ensure that there are any points to be split
            if (controlPointsToSplitAt.Count == 0)
                return;

            editorBeatmap.SelectedHitObjects.Clear();

            foreach (var splitPoint in controlPointsToSplitAt)
            {
                if (splitPoint == controlPoints[0] || splitPoint == controlPoints[^1] || splitPoint.Type == null)
                    continue;

                // Split off the section of slider before this control point so the remaining control points to split are in the latter part of the slider.
                int index = controlPoints.IndexOf(splitPoint);

                if (index <= 0)
                    continue;

                // Extract the split portion and remove from the original slider.
                var splitControlPoints = controlPoints.Take(index + 1).ToList();
                controlPoints.RemoveRange(0, index);

                var newSlider = new Slider
                {
                    StartTime = HitObject.StartTime,
                    Position = HitObject.Position + splitControlPoints[0].Position,
                    NewCombo = HitObject.NewCombo,
                    Samples = HitObject.Samples.Select(s => s.With()).ToList(),
                    RepeatCount = HitObject.RepeatCount,
                    NodeSamples = HitObject.NodeSamples.Select(n => (IList<HitSampleInfo>)n.Select(s => s.With()).ToList()).ToList(),
                    Path = new SliderPath(splitControlPoints.Select(o => new PathControlPoint(o.Position - splitControlPoints[0].Position, o == splitControlPoints[^1] ? null : o.Type)).ToArray())
                };

                // Increase the start time of the slider before adding the new slider so the new slider is immediately inserted at the correct index and internal state remains valid.
                HitObject.StartTime += split_gap;

                editorBeatmap.Add(newSlider);

                HitObject.NewCombo = false;
                HitObject.Path.ExpectedDistance.Value -= newSlider.Path.CalculatedDistance;
                HitObject.StartTime += newSlider.SpanDuration;

                // In case the remainder of the slider has no length left over, give it length anyways so we don't get a 0 length slider.
                if (HitObject.Path.ExpectedDistance.Value <= Precision.DOUBLE_EPSILON)
                {
                    HitObject.Path.ExpectedDistance.Value = null;
                }
            }

            // Once all required pieces have been split off, the original slider has the final split.
            // As a final step, we must reset its control points to have an origin of (0,0).
            Vector2 first = controlPoints[0].Position;
            foreach (var c in controlPoints)
                c.Position -= first;
            HitObject.Position += first;
        }

        // duplicated in `JuiceStreamSelectionBlueprint.convertToStream()`
        // consider extracting common helper when applying changes here
        private void convertToStream()
        {
            if (editorBeatmap == null || beatDivisor == null)
                return;

            var timingPoint = editorBeatmap.ControlPointInfo.TimingPointAt(HitObject.StartTime);
            double streamSpacing = timingPoint.BeatLength / beatDivisor.Value;

            changeHandler?.BeginChange();

            int i = 0;
            double time = HitObject.StartTime;

            while (!Precision.DefinitelyBigger(time, HitObject.GetEndTime(), 1))
            {
                // positionWithRepeats is a fractional number in the range of [0, HitObject.SpanCount()]
                // and indicates how many fractional spans of a slider have passed up to time.
                double positionWithRepeats = (time - HitObject.StartTime) / HitObject.Duration * HitObject.SpanCount();
                double pathPosition = positionWithRepeats - (int)positionWithRepeats;
                // every second span is in the reverse direction - need to reverse the path position.
                if (positionWithRepeats % 2 >= 1)
                    pathPosition = 1 - pathPosition;

                Vector2 position = HitObject.Position + HitObject.Path.PositionAt(pathPosition);

                editorBeatmap.Add(new HitCircle
                {
                    StartTime = time,
                    Position = position,
                    NewCombo = i == 0 && HitObject.NewCombo,
                    Samples = HitObject.HeadCircle.Samples.Select(s => s.With()).ToList()
                });

                i += 1;
                time = HitObject.StartTime + i * streamSpacing;
            }

            editorBeatmap.Remove(HitObject);

            changeHandler?.EndChange();
        }

        public override MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Add control point", MenuItemType.Standard, () =>
            {
                changeHandler?.BeginChange();
                addControlPoint(lastRightClickPosition);
                changeHandler?.EndChange();
            })
            {
                Hotkey = new Hotkey(new KeyCombination(InputKey.Control, InputKey.MouseLeft))
            },
            new OsuMenuItem("Convert to stream", MenuItemType.Destructive, convertToStream)
            {
                Hotkey = new Hotkey(new KeyCombination(InputKey.Control, InputKey.Shift, InputKey.F))
            },
        };

        // Always refer to the drawable object's slider body so subsequent movement deltas are calculated with updated positions.
        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.SliderBody?.ToScreenSpace(DrawableObject.SliderBody.PathOffset)
                                                             ?? BodyPiece.ToScreenSpace(BodyPiece.PathStartLocation);

        protected override Vector2[] ScreenSpaceAdditionalNodes => new[]
        {
            DrawableObject.SliderBody?.ToScreenSpace(DrawableObject.SliderBody.PathEndOffset) ?? BodyPiece.ToScreenSpace(BodyPiece.PathEndLocation)
        };

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            if (BodyPiece.ReceivePositionalInputAt(screenSpacePos) && (IsSelected || DrawableObject.Body.Alpha > 0 || DrawableObject.HeadCircle.Alpha > 0))
                return true;

            if (ControlPointVisualiser == null)
                return false;

            foreach (var p in ControlPointVisualiser.Pieces)
            {
                if (p.ReceivePositionalInputAt(screenSpacePos))
                    return true;
            }

            return false;
        }

        protected virtual SliderCircleOverlay CreateCircleOverlay(Slider slider, SliderPosition position) => new SliderCircleOverlay(slider, position);
    }
}
