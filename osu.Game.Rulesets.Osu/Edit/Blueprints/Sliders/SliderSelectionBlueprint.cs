// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
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
    public class SliderSelectionBlueprint : OsuSelectionBlueprint<Slider>
    {
        protected new DrawableSlider DrawableObject => (DrawableSlider)base.DrawableObject;

        protected SliderBodyPiece BodyPiece { get; private set; }
        protected SliderCircleOverlay HeadOverlay { get; private set; }
        protected SliderCircleOverlay TailOverlay { get; private set; }

        [CanBeNull]
        protected PathControlPointVisualiser ControlPointVisualiser { get; private set; }

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        [Resolved(CanBeNull = true)]
        private IPlacementHandler placementHandler { get; set; }

        [Resolved(CanBeNull = true)]
        private EditorBeatmap editorBeatmap { get; set; }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        [Resolved(CanBeNull = true)]
        private BindableBeatDivisor beatDivisor { get; set; }

        public override Quad SelectionQuad => BodyPiece.ScreenSpaceDrawQuad;

        private readonly BindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly IBindable<int> pathVersion = new Bindable<int>();

        public SliderSelectionBlueprint(Slider slider)
            : base(slider)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                BodyPiece = new SliderBodyPiece(),
                HeadOverlay = CreateCircleOverlay(HitObject, SliderPosition.Start),
                TailOverlay = CreateCircleOverlay(HitObject, SliderPosition.End),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlPoints.BindTo(HitObject.Path.ControlPoints);

            pathVersion.BindTo(HitObject.Path.Version);
            pathVersion.BindValueChanged(_ => editorBeatmap?.Update(HitObject));

            BodyPiece.UpdateFrom(HitObject);
        }

        public override bool HandleQuickDeletion()
        {
            var hoveredControlPoint = ControlPointVisualiser?.Pieces.FirstOrDefault(p => p.IsHovered);

            if (hoveredControlPoint == null)
                return false;

            hoveredControlPoint.IsSelected.Value = true;
            ControlPointVisualiser.DeleteSelected();
            return true;
        }

        protected override void Update()
        {
            base.Update();

            if (IsSelected)
                BodyPiece.UpdateFrom(HitObject);
        }

        protected override void OnSelected()
        {
            AddInternal(ControlPointVisualiser = new PathControlPointVisualiser(HitObject, true)
            {
                RemoveControlPointsRequested = removeControlPoints
            });

            base.OnSelected();
        }

        protected override void OnDeselected()
        {
            base.OnDeselected();

            // throw away frame buffers on deselection.
            ControlPointVisualiser?.Expire();
            ControlPointVisualiser = null;

            BodyPiece.RecyclePath();
        }

        private Vector2 rightClickPosition;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Right:
                    rightClickPosition = e.MouseDownPosition;
                    return false; // Allow right click to be handled by context menu

                case MouseButton.Left:
                    if (e.ControlPressed && IsSelected)
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

        [CanBeNull]
        private PathControlPoint placementControlPoint;

        protected override bool OnDragStart(DragStartEvent e) => placementControlPoint != null;

        protected override void OnDrag(DragEvent e)
        {
            if (placementControlPoint != null)
                placementControlPoint.Position = e.MousePosition - HitObject.Position;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (placementControlPoint != null)
            {
                placementControlPoint = null;
                changeHandler?.EndChange();
            }
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

            return false;
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

            HitObject.SnapTo(composer);

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

            // Snap the slider to the current beat divisor before checking length validity.
            HitObject.SnapTo(composer);

            // If there are 0 or 1 remaining control points, or the slider has an invalid length, it is in a degenerate form and should be deleted
            if (controlPoints.Count <= 1 || !HitObject.Path.HasValidLength)
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

        private void convertToStream()
        {
            if (editorBeatmap == null || changeHandler == null || beatDivisor == null)
                return;

            var timingPoint = editorBeatmap.ControlPointInfo.TimingPointAt(HitObject.StartTime);
            double streamSpacing = timingPoint.BeatLength / beatDivisor.Value;

            changeHandler.BeginChange();

            int i = 0;
            double time = HitObject.StartTime;

            while (!Precision.DefinitelyBigger(time, HitObject.GetEndTime(), 1))
            {
                // positionWithRepeats is a fractional number in the range of [0, HitObject.SpanCount()]
                // and indicates how many fractional spans of a slider have passed up to time.
                double positionWithRepeats = (time - HitObject.StartTime) / HitObject.Duration * HitObject.SpanCount();
                double pathPosition = positionWithRepeats - (int)positionWithRepeats;
                // every second span is in the reverse direction - need to reverse the path position.
                if (Precision.AlmostBigger(positionWithRepeats % 2, 1))
                    pathPosition = 1 - pathPosition;

                Vector2 position = HitObject.Position + HitObject.Path.PositionAt(pathPosition);

                var samplePoint = (SampleControlPoint)HitObject.SampleControlPoint.DeepClone();
                samplePoint.Time = time;

                editorBeatmap.Add(new HitCircle
                {
                    StartTime = time,
                    Position = position,
                    NewCombo = i == 0 && HitObject.NewCombo,
                    SampleControlPoint = samplePoint,
                    Samples = HitObject.HeadCircle.Samples.Select(s => s.With()).ToList()
                });

                i += 1;
                time = HitObject.StartTime + i * streamSpacing;
            }

            editorBeatmap.Remove(HitObject);

            changeHandler.EndChange();
        }

        public override MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Add control point", MenuItemType.Standard, () => addControlPoint(rightClickPosition)),
            new OsuMenuItem("Convert to stream", MenuItemType.Destructive, convertToStream),
        };

        // Always refer to the drawable object's slider body so subsequent movement deltas are calculated with updated positions.
        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.SliderBody?.ToScreenSpace(DrawableObject.SliderBody.PathOffset)
                                                             ?? BodyPiece.ToScreenSpace(BodyPiece.PathStartLocation);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            BodyPiece.ReceivePositionalInputAt(screenSpacePos) || ControlPointVisualiser?.Pieces.Any(p => p.ReceivePositionalInputAt(screenSpacePos)) == true;

        protected virtual SliderCircleOverlay CreateCircleOverlay(Slider slider, SliderPosition position) => new SliderCircleOverlay(slider, position);
    }
}
