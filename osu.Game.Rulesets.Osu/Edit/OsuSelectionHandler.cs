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
        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider? snapProvider { get; set; }

        /// <summary>
        /// During a transform, the initial path types of a single selected slider are stored so they
        /// can be maintained throughout the operation.
        /// </summary>
        private List<PathType?>? referencePathTypes;

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            Quad quad = selectedMovableObjects.Length > 0 ? GeometryUtils.GetSurroundingQuad(selectedMovableObjects) : new Quad();

            SelectionBox.CanFlipX = SelectionBox.CanScaleX = quad.Width > 0;
            SelectionBox.CanFlipY = SelectionBox.CanScaleY = quad.Height > 0;
            SelectionBox.CanScaleDiagonally = SelectionBox.CanScaleX && SelectionBox.CanScaleY;
            SelectionBox.CanReverse = EditorBeatmap.SelectedHitObjects.Count > 1 || EditorBeatmap.SelectedHitObjects.Any(s => s is Slider);
        }

        protected override void OnOperationEnded()
        {
            base.OnOperationEnded();
            referencePathTypes = null;
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == Key.M && e.ControlPressed && e.ShiftPressed)
            {
                mergeSelection();
                return true;
            }

            return false;
        }

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            var hitObjects = selectedMovableObjects;

            // this will potentially move the selection out of bounds...
            foreach (var h in hitObjects)
                h.Position += this.ScreenSpaceDeltaToParentSpace(moveEvent.ScreenSpaceDelta);

            // but this will be corrected.
            moveSelectionInBounds();
            return true;
        }

        public override bool HandleReverse()
        {
            var hitObjects = EditorBeatmap.SelectedHitObjects;

            double endTime = hitObjects.Max(h => h.GetEndTime());
            double startTime = hitObjects.Min(h => h.StartTime);

            bool moreThanOneObject = hitObjects.Count > 1;

            var newComboPlaces = hitObjects
                .OfType<OsuHitObject>()
                .Where(h => h.NewCombo)
                .Select(obj => obj.StartTime)
                .ToList();

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

            foreach (var h in hitObjects)
            {
                if (h is OsuHitObject obj) obj.NewCombo = false;
            }

            foreach (double place in newComboPlaces)
            {
                hitObjects
                    .OfType<OsuHitObject>()
                    .Where(obj => obj.StartTime == place)
                    .ToList()
                    .ForEach(obj => obj.NewCombo = true);
            }

            return true;
        }

        public override bool HandleFlip(Direction direction, bool flipOverOrigin)
        {
            var hitObjects = selectedMovableObjects;

            var flipQuad = flipOverOrigin ? new Quad(0, 0, OsuPlayfield.BASE_SIZE.X, OsuPlayfield.BASE_SIZE.Y) : GeometryUtils.GetSurroundingQuad(hitObjects);

            bool didFlip = false;

            foreach (var h in hitObjects)
            {
                var flippedPosition = GeometryUtils.GetFlippedPosition(direction, flipQuad, h.Position);

                if (!Precision.AlmostEquals(flippedPosition, h.Position))
                {
                    h.Position = flippedPosition;
                    didFlip = true;
                }

                if (h is Slider slider)
                {
                    didFlip = true;

                    foreach (var cp in slider.Path.ControlPoints)
                    {
                        cp.Position = new Vector2(
                            (direction == Direction.Horizontal ? -1 : 1) * cp.Position.X,
                            (direction == Direction.Vertical ? -1 : 1) * cp.Position.Y
                        );
                    }
                }
            }

            return didFlip;
        }

        public override bool HandleScale(Vector2 scale, Anchor reference)
        {
            adjustScaleFromAnchor(ref scale, reference);

            var hitObjects = selectedMovableObjects;

            // for the time being, allow resizing of slider paths only if the slider is
            // the only hit object selected. with a group selection, it's likely the user
            // is not looking to change the duration of the slider but expand the whole pattern.
            if (hitObjects.Length == 1 && hitObjects.First() is Slider slider)
                scaleSlider(slider, scale);
            else
                scaleHitObjects(hitObjects, reference, scale);

            moveSelectionInBounds();
            return true;
        }

        private static void adjustScaleFromAnchor(ref Vector2 scale, Anchor reference)
        {
            // cancel out scale in axes we don't care about (based on which drag handle was used).
            if ((reference & Anchor.x1) > 0) scale.X = 0;
            if ((reference & Anchor.y1) > 0) scale.Y = 0;

            // reverse the scale direction if dragging from top or left.
            if ((reference & Anchor.x0) > 0) scale.X = -scale.X;
            if ((reference & Anchor.y0) > 0) scale.Y = -scale.Y;
        }

        public override SelectionRotationHandler CreateRotationHandler() => new OsuSelectionRotationHandler();

        private void scaleSlider(Slider slider, Vector2 scale)
        {
            referencePathTypes ??= slider.Path.ControlPoints.Select(p => p.Type).ToList();

            Quad sliderQuad = GeometryUtils.GetSurroundingQuad(slider.Path.ControlPoints.Select(p => p.Position));

            // Limit minimum distance between control points after scaling to almost 0. Less than 0 causes the slider to flip, exactly 0 causes a crash through division by 0.
            scale = Vector2.ComponentMax(new Vector2(Precision.FLOAT_EPSILON), sliderQuad.Size + scale) - sliderQuad.Size;

            Vector2 pathRelativeDeltaScale = new Vector2(
                sliderQuad.Width == 0 ? 0 : 1 + scale.X / sliderQuad.Width,
                sliderQuad.Height == 0 ? 0 : 1 + scale.Y / sliderQuad.Height);

            Queue<Vector2> oldControlPoints = new Queue<Vector2>();

            foreach (var point in slider.Path.ControlPoints)
            {
                oldControlPoints.Enqueue(point.Position);
                point.Position *= pathRelativeDeltaScale;
            }

            // Maintain the path types in case they were defaulted to bezier at some point during scaling
            for (int i = 0; i < slider.Path.ControlPoints.Count; ++i)
                slider.Path.ControlPoints[i].Type = referencePathTypes[i];

            // Snap the slider's length to the current beat divisor
            // to calculate the final resulting duration / bounding box before the final checks.
            slider.SnapTo(snapProvider);

            //if sliderhead or sliderend end up outside playfield, revert scaling.
            Quad scaledQuad = GeometryUtils.GetSurroundingQuad(new OsuHitObject[] { slider });
            (bool xInBounds, bool yInBounds) = isQuadInBounds(scaledQuad);

            if (xInBounds && yInBounds && slider.Path.HasValidLength)
                return;

            foreach (var point in slider.Path.ControlPoints)
                point.Position = oldControlPoints.Dequeue();

            // Snap the slider's length again to undo the potentially-invalid length applied by the previous snap.
            slider.SnapTo(snapProvider);
        }

        private void scaleHitObjects(OsuHitObject[] hitObjects, Anchor reference, Vector2 scale)
        {
            scale = getClampedScale(hitObjects, reference, scale);
            Quad selectionQuad = GeometryUtils.GetSurroundingQuad(hitObjects);

            foreach (var h in hitObjects)
                h.Position = GeometryUtils.GetScaledPosition(reference, scale, selectionQuad, h.Position);
        }

        private (bool X, bool Y) isQuadInBounds(Quad quad)
        {
            bool xInBounds = (quad.TopLeft.X >= 0) && (quad.BottomRight.X <= DrawWidth);
            bool yInBounds = (quad.TopLeft.Y >= 0) && (quad.BottomRight.Y <= DrawHeight);

            return (xInBounds, yInBounds);
        }

        private void moveSelectionInBounds()
        {
            var hitObjects = selectedMovableObjects;

            Quad quad = GeometryUtils.GetSurroundingQuad(hitObjects);

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
        /// Clamp scale for multi-object-scaling where selection does not exceed playfield bounds or flip.
        /// </summary>
        /// <param name="hitObjects">The hitobjects to be scaled</param>
        /// <param name="reference">The anchor from which the scale operation is performed</param>
        /// <param name="scale">The scale to be clamped</param>
        /// <returns>The clamped scale vector</returns>
        private Vector2 getClampedScale(OsuHitObject[] hitObjects, Anchor reference, Vector2 scale)
        {
            float xOffset = ((reference & Anchor.x0) > 0) ? -scale.X : 0;
            float yOffset = ((reference & Anchor.y0) > 0) ? -scale.Y : 0;

            Quad selectionQuad = GeometryUtils.GetSurroundingQuad(hitObjects);

            //todo: this is not always correct for selections involving sliders. This approximation assumes each point is scaled independently, but sliderends move with the sliderhead.
            Quad scaledQuad = new Quad(selectionQuad.TopLeft.X + xOffset, selectionQuad.TopLeft.Y + yOffset, selectionQuad.Width + scale.X, selectionQuad.Height + scale.Y);

            //max Size -> playfield bounds
            if (scaledQuad.TopLeft.X < 0)
                scale.X += scaledQuad.TopLeft.X;
            if (scaledQuad.TopLeft.Y < 0)
                scale.Y += scaledQuad.TopLeft.Y;

            if (scaledQuad.BottomRight.X > DrawWidth)
                scale.X -= scaledQuad.BottomRight.X - DrawWidth;
            if (scaledQuad.BottomRight.Y > DrawHeight)
                scale.Y -= scaledQuad.BottomRight.Y - DrawHeight;

            //min Size -> almost 0. Less than 0 causes the quad to flip, exactly 0 causes scaling to get stuck at minimum scale.
            Vector2 scaledSize = selectionQuad.Size + scale;
            Vector2 minSize = new Vector2(Precision.FLOAT_EPSILON);

            scale = Vector2.ComponentMax(minSize, scaledSize) - selectionQuad.Size;

            return scale;
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
