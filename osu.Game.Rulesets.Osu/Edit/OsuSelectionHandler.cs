// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Extensions;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Utils;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit
{
    public partial class OsuSelectionHandler : EditorSelectionHandler
    {
        [Resolved]
        private OsuGridToolboxGroup gridToolbox { get; set; } = null!;

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            Quad quad = selectedMovableObjects.Length > 0 ? GeometryUtils.GetSurroundingQuad(selectedMovableObjects) : new Quad();

            SelectionBox.CanFlipX = quad.Width > 0;
            SelectionBox.CanFlipY = quad.Height > 0;
            SelectionBox.CanReverse = EditorBeatmap.SelectedHitObjects.Count > 1 || EditorBeatmap.SelectedHitObjects.Any(s => s is Slider);
        }

        private bool nudgeMovementActive;

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key.M && e.ControlPressed && e.ShiftPressed)
            {
                mergeSelection();
                return true;
            }

            // Until the keys below are global actions, this will prevent conflicts with "seek between sample points"
            // which has a default of ctrl+shift+arrows.
            if (e.ShiftPressed)
                return false;

            if (e.ControlPressed)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        return nudgeSelection(new Vector2(-1, 0));

                    case Key.Right:
                        return nudgeSelection(new Vector2(1, 0));

                    case Key.Up:
                        return nudgeSelection(new Vector2(0, -1));

                    case Key.Down:
                        return nudgeSelection(new Vector2(0, 1));
                }
            }

            return false;
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            base.OnKeyUp(e);

            if (nudgeMovementActive && !e.ControlPressed)
            {
                EditorBeatmap.EndChange();
                nudgeMovementActive = false;
            }
        }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            var hitObjects = selectedMovableObjects;

            var localDelta = this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);

            // this conditional is a rather ugly special case for stacks.
            // as it turns out, adding the `EditorBeatmap.Update()` call at the end of this would cause stacked objects to jitter when moved around
            // (they would stack and then unstack every frame).
            // the reason for that is that the selection handling abstractions are not aware of the distinction between "displayed" and "actual" position
            // which is unique to osu! due to stacking being applied as a post-processing step.
            // therefore, the following loop would occur:
            // - on frame 1 the blueprint is snapped to the stack's baseline position. `EditorBeatmap.Update()` applies stacking successfully,
            //   the blueprint moves up the stack from its original drag position.
            // - on frame 2 the blueprint's position is now the *stacked* position, which is interpreted higher up as *manually performing an unstack*
            //   to the blueprint's unstacked position (as the machinery higher up only cares about differences in screen space position).
            if (hitObjects.Any(h => Precision.AlmostEquals(localDelta, -h.StackOffset)))
                return true;

            moveObjects(hitObjects, localDelta);

            return true;
        }

        private void moveObjects(OsuHitObject[] hitObjects, Vector2 localDelta)
        {
            // this will potentially move the selection out of bounds...
            foreach (var h in hitObjects)
                h.Position += localDelta;

            // but this will be corrected.
            moveSelectionInBounds();

            // manually update stacking.
            // this intentionally bypasses the editor `UpdateState()` / beatmap processor flow for performance reasons,
            // as the entire flow is too expensive to run on every movement.
            Scheduler.AddOnce(OsuBeatmapProcessor.ApplyStacking, EditorBeatmap);
        }

        /// <summary>
        /// Move the current selection spatially by the specified delta, in gamefield coordinates (ie. the same coordinates as the blueprints).
        /// </summary>
        /// <param name="delta"></param>
        private bool nudgeSelection(Vector2 delta)
        {
            if (!nudgeMovementActive)
            {
                nudgeMovementActive = true;
                EditorBeatmap.BeginChange();
            }

            var firstBlueprint = SelectedBlueprints.FirstOrDefault();

            if (firstBlueprint == null)
                return false;

            moveObjects(selectedMovableObjects, delta);
            return true;
        }

        public override bool HandleReverse()
        {
            var hitObjects = EditorBeatmap.SelectedHitObjects
                                          .OfType<OsuHitObject>()
                                          .OrderBy(obj => obj.StartTime)
                                          .ToList();

            double endTime = hitObjects.Max(h => h.GetEndTime());
            double startTime = hitObjects.Min(h => h.StartTime);

            bool moreThanOneObject = hitObjects.Count > 1;

            // the expectation is that even if the objects themselves are reversed temporally,
            // the position of new combos in the selection should remain the same.
            // preserve it for later before doing the reversal.
            var newComboOrder = hitObjects.Select(obj => obj.NewCombo).ToList();

            foreach (var h in hitObjects)
            {
                if (moreThanOneObject)
                    h.StartTime = endTime - (h.GetEndTime() - startTime);

                if (h is Slider slider)
                {
                    slider.Path.Reverse(out Vector2 offset);
                    slider.Position += offset;
                }
            }

            // re-order objects by start time again after reversing, and restore new combo flag positioning
            hitObjects = hitObjects.OrderBy(obj => obj.StartTime).ToList();

            for (int i = 0; i < hitObjects.Count; ++i)
                hitObjects[i].NewCombo = newComboOrder[i];

            return true;
        }

        public override bool HandleFlip(Direction direction, bool flipOverOrigin)
        {
            var hitObjects = selectedMovableObjects;

            // If we're flipping over the origin, we take the grid origin position from the grid toolbox.
            var flipQuad = flipOverOrigin ? new Quad(gridToolbox.StartPositionX.Value, gridToolbox.StartPositionY.Value, 0, 0) : GeometryUtils.GetSurroundingQuad(hitObjects);
            Vector2 flipAxis = direction == Direction.Vertical ? Vector2.UnitY : Vector2.UnitX;

            if (flipOverOrigin)
            {
                // If we're flipping over the origin, we take one of the axes of the grid.
                // Take the axis closest to the direction we want to flip over.
                switch (gridToolbox.GridType.Value)
                {
                    case PositionSnapGridType.Square:
                        flipAxis = GeometryUtils.RotateVector(Vector2.UnitX, -((gridToolbox.GridLinesRotation.Value + 360 + 45) % 90 - 45));
                        flipAxis = direction == Direction.Vertical ? flipAxis.PerpendicularLeft : flipAxis;
                        break;

                    case PositionSnapGridType.Triangle:
                        // Hex grid has 3 axes, so you can not directly flip over one of the axes,
                        // however it's still possible to achieve that flip by combining multiple flips over the other axes.
                        // Angle degree range for vertical = (-120, -60]
                        // Angle degree range for horizontal = [-30, 30)
                        flipAxis = direction == Direction.Vertical
                            ? GeometryUtils.RotateVector(Vector2.UnitX, -((gridToolbox.GridLinesRotation.Value + 360 + 30) % 60 + 60))
                            : GeometryUtils.RotateVector(Vector2.UnitX, -((gridToolbox.GridLinesRotation.Value + 360) % 60 - 30));
                        break;
                }
            }

            var controlPointFlipQuad = new Quad();

            bool didFlip = false;

            foreach (var h in hitObjects)
            {
                var flippedPosition = GeometryUtils.GetFlippedPosition(flipAxis, flipQuad, h.Position);

                // Clamp the flipped position inside the playfield bounds, because the flipped position might be outside the playfield bounds if the origin is not centered.
                flippedPosition = Vector2.Clamp(flippedPosition, Vector2.Zero, OsuPlayfield.BASE_SIZE);

                if (!Precision.AlmostEquals(flippedPosition, h.Position))
                {
                    h.Position = flippedPosition;
                    didFlip = true;
                }

                if (h is Slider slider)
                {
                    didFlip = true;

                    foreach (var cp in slider.Path.ControlPoints)
                        cp.Position = GeometryUtils.GetFlippedPosition(flipAxis, controlPointFlipQuad, cp.Position);
                }
            }

            return didFlip;
        }

        public override SelectionRotationHandler CreateRotationHandler() => new OsuSelectionRotationHandler();

        public override SelectionScaleHandler CreateScaleHandler() => new OsuSelectionScaleHandler();

        private void moveSelectionInBounds()
        {
            var hitObjects = selectedMovableObjects;

            Quad quad = GeometryUtils.GetSurroundingQuad(hitObjects, true);

            Vector2 delta = Vector2.Zero;

            if (quad.TopLeft.X < 0)
                delta.X -= quad.TopLeft.X;
            if (quad.TopLeft.Y < 0)
                delta.Y -= quad.TopLeft.Y;

            if (quad.BottomRight.X > DrawWidth)
                delta.X -= quad.BottomRight.X - DrawWidth;
            if (quad.BottomRight.Y > DrawHeight)
                delta.Y -= quad.BottomRight.Y - DrawHeight;

            foreach (var h in hitObjects)
                h.Position += delta;
        }

        /// <summary>
        /// All osu! hitobjects which can be moved/rotated/scaled.
        /// </summary>
        private OsuHitObject[] selectedMovableObjects => SelectedItems.OfType<OsuHitObject>()
                                                                      .Where(h => h is not Spinner)
                                                                      .ToArray();

        /// <summary>
        /// All osu! hitobjects which can be merged.
        /// </summary>
        private OsuHitObject[] selectedMergeableObjects => SelectedItems.OfType<OsuHitObject>()
                                                                        .Where(h => h is HitCircle or Slider)
                                                                        .OrderBy(h => h.StartTime)
                                                                        .ToArray();

        private void mergeSelection()
        {
            var mergeableObjects = selectedMergeableObjects;

            if (!canMerge(mergeableObjects))
                return;

            EditorBeatmap.BeginChange();

            // Have an initial slider object.
            var firstHitObject = mergeableObjects[0];
            var mergedHitObject = firstHitObject as Slider ?? new Slider
            {
                StartTime = firstHitObject.StartTime,
                Position = firstHitObject.Position,
                NewCombo = firstHitObject.NewCombo,
                Samples = firstHitObject.Samples,
            };

            if (mergedHitObject.Path.ControlPoints.Count == 0)
            {
                mergedHitObject.Path.ControlPoints.Add(new PathControlPoint(Vector2.Zero, PathType.LINEAR));
            }

            // Merge all the selected hit objects into one slider path.
            bool lastCircle = firstHitObject is HitCircle;

            foreach (var selectedMergeableObject in mergeableObjects.Skip(1))
            {
                if (selectedMergeableObject is IHasPath hasPath)
                {
                    var offset = lastCircle ? selectedMergeableObject.Position - mergedHitObject.Position : mergedHitObject.Path.ControlPoints[^1].Position;
                    float distanceToLastControlPoint = Vector2.Distance(mergedHitObject.Path.ControlPoints[^1].Position, offset);

                    // Calculate the distance required to travel to the expected distance of the merging slider.
                    mergedHitObject.Path.ExpectedDistance.Value = mergedHitObject.Path.CalculatedDistance + distanceToLastControlPoint + hasPath.Path.Distance;

                    // Remove the last control point if it sits exactly on the start of the next control point.
                    if (Precision.AlmostEquals(distanceToLastControlPoint, 0))
                    {
                        mergedHitObject.Path.ControlPoints.RemoveAt(mergedHitObject.Path.ControlPoints.Count - 1);
                    }

                    mergedHitObject.Path.ControlPoints.AddRange(hasPath.Path.ControlPoints.Select(o => new PathControlPoint(o.Position + offset, o.Type)));
                    lastCircle = false;
                }
                else
                {
                    // Turn the last control point into a linear type if this is the first merging circle in a sequence, so the subsequent control points can be inherited path type.
                    if (!lastCircle)
                    {
                        mergedHitObject.Path.ControlPoints.Last().Type = PathType.LINEAR;
                    }

                    mergedHitObject.Path.ControlPoints.Add(new PathControlPoint(selectedMergeableObject.Position - mergedHitObject.Position));
                    mergedHitObject.Path.ExpectedDistance.Value = null;
                    lastCircle = true;
                }
            }

            // Make sure only the merged hit object is in the beatmap.
            if (firstHitObject is Slider)
            {
                foreach (var selectedMergeableObject in mergeableObjects.Skip(1))
                {
                    EditorBeatmap.Remove(selectedMergeableObject);
                }
            }
            else
            {
                foreach (var selectedMergeableObject in mergeableObjects)
                {
                    EditorBeatmap.Remove(selectedMergeableObject);
                }

                EditorBeatmap.Add(mergedHitObject);
            }

            // Make sure the merged hitobject is selected.
            SelectedItems.Clear();
            SelectedItems.Add(mergedHitObject);

            EditorBeatmap.EndChange();
        }

        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
        {
            foreach (var item in base.GetContextMenuItemsForSelection(selection))
                yield return item;

            if (canMerge(selectedMergeableObjects))
                yield return new OsuMenuItem("Merge selection", MenuItemType.Destructive, mergeSelection);
        }

        private bool canMerge(IReadOnlyList<OsuHitObject> objects) =>
            objects.Count > 1
            && (objects.Any(h => h is Slider)
                || objects.Zip(objects.Skip(1), (h1, h2) => Precision.DefinitelyBigger(Vector2.DistanceSquared(h1.Position, h2.Position), 1)).Any(x => x));
    }
}
