﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
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
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderSelectionBlueprint : OsuSelectionBlueprint<Slider>
    {
        protected readonly SliderBodyPiece BodyPiece;
        protected readonly SliderCircleSelectionBlueprint HeadBlueprint;
        protected readonly SliderCircleSelectionBlueprint TailBlueprint;
        protected readonly PathControlPointVisualiser ControlPointVisualiser;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        [Resolved(CanBeNull = true)]
        private IPlacementHandler placementHandler { get; set; }

        public SliderSelectionBlueprint(DrawableSlider slider)
            : base(slider)
        {
            var sliderObject = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                BodyPiece = new SliderBodyPiece(),
                HeadBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.Start),
                TailBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.End),
                ControlPointVisualiser = new PathControlPointVisualiser(sliderObject, true)
                {
                    RemoveControlPointsRequested = removeControlPoints
                }
            };
        }

        private IBindable<int> pathVersion;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            pathVersion = HitObject.Path.Version.GetBoundCopy();
            pathVersion.BindValueChanged(_ => updatePath());
        }

        protected override void Update()
        {
            base.Update();

            BodyPiece.UpdateFrom(HitObject);
        }

        private Vector2 rightClickPosition;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            switch (e.Button)
            {
                case MouseButton.Right:
                    rightClickPosition = e.MouseDownPosition;
                    return false; // Allow right click to be handled by context menu

                case MouseButton.Left when e.ControlPressed && IsSelected:
                    placementControlPointIndex = addControlPoint(e.MousePosition);
                    return true; // Stop input from being handled and modifying the selection
            }

            return false;
        }

        private int? placementControlPointIndex;

        protected override bool OnDragStart(DragStartEvent e) => placementControlPointIndex != null;

        protected override void OnDrag(DragEvent e)
        {
            Debug.Assert(placementControlPointIndex != null);

            HitObject.Path.ControlPoints[placementControlPointIndex.Value].Position.Value = e.MousePosition - HitObject.Position;
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            placementControlPointIndex = null;
        }

        private BindableList<PathControlPoint> controlPoints => HitObject.Path.ControlPoints;

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
            UpdateHitObject();
        }

        public override MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Add control point", MenuItemType.Standard, () => addControlPoint(rightClickPosition)),
        };

        public override Vector2 SelectionPoint => ((DrawableSlider)DrawableObject).HeadCircle.ScreenSpaceDrawQuad.Centre;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => BodyPiece.ReceivePositionalInputAt(screenSpacePos);

        protected virtual SliderCircleSelectionBlueprint CreateCircleSelectionBlueprint(DrawableSlider slider, SliderPosition position) => new SliderCircleSelectionBlueprint(slider, position);
    }
}
