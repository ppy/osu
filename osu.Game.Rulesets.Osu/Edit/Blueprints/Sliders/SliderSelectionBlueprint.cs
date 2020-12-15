// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
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
        protected SliderBodyPiece BodyPiece { get; private set; }
        protected SliderCircleSelectionBlueprint HeadBlueprint { get; private set; }
        protected SliderCircleSelectionBlueprint TailBlueprint { get; private set; }
        protected PathControlPointVisualiser ControlPointVisualiser { get; private set; }

        private readonly DrawableSlider slider;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        [Resolved(CanBeNull = true)]
        private IPlacementHandler placementHandler { get; set; }

        [Resolved(CanBeNull = true)]
        private EditorBeatmap editorBeatmap { get; set; }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        private readonly BindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly IBindable<int> pathVersion = new Bindable<int>();

        public SliderSelectionBlueprint(DrawableSlider slider)
            : base(slider)
        {
            this.slider = slider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                BodyPiece = new SliderBodyPiece(),
                HeadBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.Start),
                TailBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.End),
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlPoints.BindTo(HitObject.Path.ControlPoints);

            pathVersion.BindTo(HitObject.Path.Version);
            pathVersion.BindValueChanged(_ => updatePath());

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
            AddInternal(ControlPointVisualiser = new PathControlPointVisualiser(slider.HitObject, true)
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
                        placementControlPointIndex = addControlPoint(e.MousePosition);
                        return true; // Stop input from being handled and modifying the selection
                    }

                    break;
            }

            return false;
        }

        private int? placementControlPointIndex;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (placementControlPointIndex != null)
            {
                changeHandler?.BeginChange();
                return true;
            }

            return false;
        }

        protected override void OnDrag(DragEvent e)
        {
            Debug.Assert(placementControlPointIndex != null);

            HitObject.Path.ControlPoints[placementControlPointIndex.Value].Position.Value = e.MousePosition - HitObject.Position;
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            if (placementControlPointIndex != null)
            {
                placementControlPointIndex = null;
                changeHandler?.EndChange();
            }
        }

        private int addControlPoint(Vector2 position)
        {
            position -= HitObject.Position;

            int insertionIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                float dist = new Line(controlPoints[i].Position.Value, controlPoints[i + 1].Position.Value).DistanceToPoint(position);

                if (dist < minDistance)
                {
                    insertionIndex = i + 1;
                    minDistance = dist;
                }
            }

            // Move the control points from the insertion index onwards to make room for the insertion
            controlPoints.Insert(insertionIndex, new PathControlPoint { Position = { Value = position } });

            return insertionIndex;
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
                if (c == controlPoints[0] && controlPoints.Count > 1 && controlPoints[1].Type.Value == null)
                    controlPoints[1].Type.Value = controlPoints[0].Type.Value;

                controlPoints.Remove(c);
            }

            // If there are 0 or 1 remaining control points, the slider is in a degenerate (single point) form and should be deleted
            if (controlPoints.Count <= 1)
            {
                placementHandler?.Delete(HitObject);
                return;
            }

            // The path will have a non-zero offset if the head is removed, but sliders don't support this behaviour since the head is positioned at the slider's position
            // So the slider needs to be offset by this amount instead, and all control points offset backwards such that the path is re-positioned at (0, 0)
            Vector2 first = controlPoints[0].Position.Value;
            foreach (var c in controlPoints)
                c.Position.Value -= first;
            HitObject.Position += first;
        }

        private void updatePath()
        {
            HitObject.Path.ExpectedDistance.Value = composer?.GetSnappedDistanceFromDistance(HitObject.StartTime, (float)HitObject.Path.CalculatedDistance) ?? (float)HitObject.Path.CalculatedDistance;
            editorBeatmap?.Update(HitObject);
        }

        public override MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Add control point", MenuItemType.Standard, () => addControlPoint(rightClickPosition)),
        };

        public override Vector2 ScreenSpaceSelectionPoint => BodyPiece.ToScreenSpace(BodyPiece.PathStartLocation);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            BodyPiece.ReceivePositionalInputAt(screenSpacePos) || ControlPointVisualiser?.Pieces.Any(p => p.ReceivePositionalInputAt(screenSpacePos)) == true;

        protected virtual SliderCircleSelectionBlueprint CreateCircleSelectionBlueprint(DrawableSlider slider, SliderPosition position) => new SliderCircleSelectionBlueprint(slider, position);
    }
}
