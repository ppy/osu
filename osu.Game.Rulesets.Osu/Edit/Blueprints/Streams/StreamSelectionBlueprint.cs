// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Streams.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Streams
{
    public class StreamSelectionBlueprint : OsuSelectionBlueprint<Stream>
    {
        protected new DrawableStream DrawableObject => (DrawableStream)base.DrawableObject;

        protected StreamPiece StreamPiece { get; private set; } = null!;

        protected PathControlPointVisualiser<Stream>? ControlPointVisualiser { get; private set; }

        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider? snapProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private IPlacementHandler? placementHandler { get; set; }

        [Resolved(CanBeNull = true)]
        private EditorBeatmap? editorBeatmap { get; set; }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler? changeHandler { get; set; }

        [Resolved(CanBeNull = true)]
        private BindableBeatDivisor? beatDivisor { get; set; }

        public override Quad SelectionQuad => StreamPiece.ScreenSpaceDrawQuad;

        private readonly BindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly IBindable<int> pathVersion = new Bindable<int>();
        private readonly BindableList<HitObject> selectedObjects = new BindableList<HitObject>();

        public StreamSelectionBlueprint(Stream stream)
            : base(stream)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = StreamPiece = new StreamPiece();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            controlPoints.BindTo(HitObject.Path.ControlPoints);

            pathVersion.BindTo(HitObject.Path.Version);
            pathVersion.BindValueChanged(_ => editorBeatmap?.Update(HitObject));

            StreamPiece.UpdateFrom(HitObject);

            if (editorBeatmap != null)
                selectedObjects.BindTo(editorBeatmap.SelectedHitObjects);
            selectedObjects.BindCollectionChanged((_, _) => updateVisualDefinition(), true);
        }

        public override bool HandleQuickDeletion()
        {
            var hoveredControlPoint = ControlPointVisualiser?.Pieces.FirstOrDefault(p => p.IsHovered);

            if (hoveredControlPoint == null)
                return false;

            hoveredControlPoint.IsSelected.Value = true;
            ControlPointVisualiser?.DeleteSelected();
            return true;
        }

        private bool hasSingleObjectSelected => selectedObjects.Count == 1;

        protected override void Update()
        {
            base.Update();

            if (IsSelected)
                StreamPiece.UpdateFrom(HitObject);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateVisualDefinition();

            // In the case more than a single object is selected, block hover from arriving at sliders behind this one.
            // Without doing this, the path visualisers of potentially hundreds of sliders will render, which is not only
            // visually noisy but also functionally useless.
            return !hasSingleObjectSelected;
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

            updateVisualDefinition();
        }

        private void updateVisualDefinition()
        {
            // To reduce overhead of drawing these blueprints, only add extra detail when hovered or when only this slider is selected.
            if (IsSelected && (hasSingleObjectSelected || IsHovered))
            {
                if (ControlPointVisualiser == null)
                {
                    AddInternal(ControlPointVisualiser = new PathControlPointVisualiser<Stream>(HitObject, true, false)
                    {
                        RemoveControlPointsRequested = removeControlPoints
                    });
                }
            }
            else
            {
                ControlPointVisualiser?.Expire();
                ControlPointVisualiser = null;
            }
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

        private PathControlPoint? placementControlPoint;

        protected override bool OnDragStart(DragStartEvent e) => placementControlPoint != null;

        protected override void OnDrag(DragEvent e)
        {
            if (placementControlPoint != null)
            {
                var result = snapProvider?.FindSnappedPositionAndTime(ToScreenSpace(e.MousePosition));
                placementControlPoint.Position = ToLocalSpace(result?.ScreenSpacePosition ?? ToScreenSpace(e.MousePosition)) - HitObject.Position;
            }
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
            if (editorBeatmap == null || beatDivisor == null)
                return;

            changeHandler?.BeginChange();

            editorBeatmap.AddRange(HitObject.ToHitCircles());
            editorBeatmap.Remove(HitObject);

            changeHandler?.EndChange();
        }

        public override MenuItem[] ContextMenuItems => new MenuItem[]
        {
            new OsuMenuItem("Add control point", MenuItemType.Standard, () => addControlPoint(rightClickPosition)),
            new OsuMenuItem("Convert to stream", MenuItemType.Destructive, convertToStream),
        };

        // Always refer to the drawable object's slider body so subsequent movement deltas are calculated with updated positions.
        public override Vector2 ScreenSpaceSelectionPoint => DrawableObject.ToScreenSpace(DrawableObject.OriginPosition);

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            StreamPiece.ReceivePositionalInputAt(screenSpacePos) || ControlPointVisualiser?.Pieces.Any(p => p.ReceivePositionalInputAt(screenSpacePos)) == true;
    }
}
