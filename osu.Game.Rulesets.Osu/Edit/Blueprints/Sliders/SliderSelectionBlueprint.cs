// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderSelectionBlueprint : OsuSelectionBlueprint<Slider>, IHasContextMenu
    {
        protected readonly SliderBodyPiece BodyPiece;
        protected readonly SliderCircleSelectionBlueprint HeadBlueprint;
        protected readonly SliderCircleSelectionBlueprint TailBlueprint;
        protected readonly PathControlPointVisualiser ControlPointVisualiser;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        public SliderSelectionBlueprint(DrawableSlider slider)
            : base(slider)
        {
            var sliderObject = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                BodyPiece = new SliderBodyPiece(),
                HeadBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.Start),
                TailBlueprint = CreateCircleSelectionBlueprint(slider, SliderPosition.End),
                ControlPointVisualiser = new PathControlPointVisualiser(sliderObject, true) { ControlPointsChanged = onNewControlPoints },
            };
        }

        protected override void Update()
        {
            base.Update();

            BodyPiece.UpdateFrom(HitObject);
        }

        private Vector2 rightClickPosition;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Right)
                rightClickPosition = e.MouseDownPosition;

            return false;
        }

        private void addControlPoint()
        {
            Vector2 position = rightClickPosition - HitObject.Position;

            var controlPoints = new Vector2[HitObject.Path.ControlPoints.Length + 1];
            HitObject.Path.ControlPoints.CopyTo(controlPoints);

            // Find the index at which the point will be inserted, by increasing x-coordinates
            int insertionIndex = Array.FindIndex(controlPoints, 0, controlPoints.Length - 1, c => c.X >= position.X);

            // If no index was found, it should be inserted at the end
            if (insertionIndex == -1)
                insertionIndex = controlPoints.Length - 1;

            // Move the control points from the insertion index onwards to make room for the insertion
            Array.Copy(controlPoints, insertionIndex, controlPoints, insertionIndex + 1, controlPoints.Length - insertionIndex - 1);

            if (insertionIndex == 0)
            {
                // Special case for a new first control point being added - the entire slider moves
                HitObject.Position += position;

                // The first control point is always at (0, 0), but all other control points need to be re-referenced
                for (int i = 1; i < controlPoints.Length; i++)
                    controlPoints[i] -= position;
            }
            else
                controlPoints[insertionIndex] = position;

            onNewControlPoints(controlPoints);
        }

        private void onNewControlPoints(Vector2[] controlPoints)
        {
            var unsnappedPath = new SliderPath(controlPoints.Length > 2 ? PathType.Bezier : PathType.Linear, controlPoints);
            var snappedDistance = composer?.GetSnappedDistanceFromDistance(HitObject.StartTime, (float)unsnappedPath.Distance) ?? (float)unsnappedPath.Distance;

            HitObject.Path = new SliderPath(unsnappedPath.Type, controlPoints, snappedDistance);

            UpdateHitObject();
        }

        public MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Add control point", MenuItemType.Standard, addControlPoint),
        };

        public override Vector2 SelectionPoint => HeadBlueprint.SelectionPoint;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => BodyPiece.ReceivePositionalInputAt(screenSpacePos);

        protected virtual SliderCircleSelectionBlueprint CreateCircleSelectionBlueprint(DrawableSlider slider, SliderPosition position) => new SliderCircleSelectionBlueprint(slider, position);
    }
}
